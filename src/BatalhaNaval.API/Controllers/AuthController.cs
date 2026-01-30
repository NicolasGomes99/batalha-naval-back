using System.IdentityModel.Tokens.Jwt;
using BatalhaNaval.Application.DTOs;
using BatalhaNaval.Application.Interfaces;
using BatalhaNaval.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BatalhaNaval.API.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<AuthController> _logger;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;

    public AuthController(
        IUserRepository userRepository,
        IPasswordService passwordService,
        ITokenService tokenService,
        ICacheService cacheService,
        ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _cacheService = cacheService;
        _logger = logger;
    }


    /// <summary>
    ///     Efetua o login de um usuário.
    /// </summary>
    /// <remarks>
    ///     Efetua o login de um usuário com nome de usuário e senha fornecidos.
    /// </remarks>
    /// <response code="200">Login efetuado com sucesso.</response>
    /// <response code="401">Nome de usuário ou senha inválidos.</response>
    [HttpPost("login", Name = "PostLogin")]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var user = await _userRepository.GetByUsernameAsync(loginDto.Username);

        if (user == null || !_passwordService.VerifyPassword(loginDto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Usuário ou senha inválidos.", StatusCode = 401 });

        var accessToken = _tokenService.GenerateAccessToken(user);

        var refreshToken = _tokenService.GenerateRefreshToken();

        await _cacheService.SetAsync($"rt:{refreshToken}", user.Id.ToString(), TimeSpan.FromDays(7));

        return Ok(new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Username = user.Username,
            Profile = user.Profile != null
                ? new UserProfileDTO
                {
                    RankPoints = user.Profile.RankPoints,
                    Wins = user.Profile.Wins,
                    Losses = user.Profile.Losses
                }
                : new UserProfileDTO()
        });
    }

    /// <summary>
    ///     Realiza o logout invalidando o token atual.
    /// </summary>
    /// <remarks>
    ///     Adiciona o JTI (ID do token) a uma blocklist no Redis até sua expiração natural.
    /// </remarks>
    /// <response code="200">Logout realizado com sucesso.</response>
    /// <response code="401">Token inválido ou não fornecido.</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] LogoutDto logoutDto)
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        var accessToken = authHeader.Replace("Bearer ", "");

        if (!string.IsNullOrEmpty(accessToken))
        {
            var handler = new JwtSecurityTokenHandler();
            if (handler.ReadToken(accessToken) is JwtSecurityToken jsonToken)
            {
                var jti = jsonToken.Id;
                var expiration = jsonToken.ValidTo;
                var timeRemaining = expiration - DateTime.UtcNow;

                if (timeRemaining > TimeSpan.Zero) await _cacheService.SetAsync($"bl:{jti}", "revoked", timeRemaining);
            }
        }

        if (!string.IsNullOrWhiteSpace(logoutDto.RefreshToken))
            await _cacheService.RemoveAsync($"rt:{logoutDto.RefreshToken}");

        return Ok(new { message = "Logout realizado com sucesso." });
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest("Refresh Token é obrigatório.");

        var userIdString = await _cacheService.GetAsync<string>($"rt:{request.RefreshToken}");

        if (string.IsNullOrEmpty(userIdString)) return Unauthorized("Refresh token inválido ou expirado.");

        try
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader))
            {
                var oldAccessToken = authHeader.Replace("Bearer ", "");
                var handler = new JwtSecurityTokenHandler();

                if (handler.CanReadToken(oldAccessToken))
                {
                    var jsonToken = handler.ReadToken(oldAccessToken) as JwtSecurityToken;

                    if (jsonToken != null)
                    {
                        var jti = jsonToken.Id;
                        var expiration = jsonToken.ValidTo;
                        var timeRemaining = expiration - DateTime.UtcNow;

                        if (timeRemaining > TimeSpan.Zero)
                            await _cacheService.SetAsync($"bl:{jti}", "revoked_via_refresh", timeRemaining);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao invalidar o token de acesso antigo durante o refresh token.");
        }

        await _cacheService.RemoveAsync($"rt:{request.RefreshToken}");

        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized(new { message = "Erro ao processar identificação do usuário." });

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return Unauthorized(new { message = "Usuário não encontrado." });

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        await _cacheService.SetAsync($"rt:{newRefreshToken}", userId, TimeSpan.FromDays(7));

        return Ok(new { AccessToken = newAccessToken, RefreshToken = newRefreshToken });
    }
}