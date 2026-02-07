using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BatalhaNaval.Application.DTOs;
using FluentAssertions;
using FluentAssertions.Execution;

namespace BatalhaNaval.IntegrationTests;

[Collection("Sequential")]
public class AuthTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private const string EndpointRegister = "/users";
    private const string EndpointProfile = "/users/profile";

    private const string EndpointLogin = "/auth/login";
    private const string EndpointRefreshToken = "/auth/refresh-token";
    private const string EndpointLogout = "/auth/logout";
    private readonly HttpClient _client;

    public AuthTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Deve_Executar_Ciclo_De_Vida_Completo_Autenticacao_E_Seguranca()
    {
        var usuario = new { Username = "AlmiranteAuth", Password = "SenhaSeguraAuth123!" };

        // STEP 1: Segurança - Tentar logar com usuário que não existe
        await Passo_TentarLoginUsuarioInexistente();

        // STEP 2: Registrar um usuário válido
        await Passo_RegistrarUsuario(usuario);

        // STEP 3: Login com Sucesso
        var tokensIniciais = await Passo_RealizarLoginComSucesso(usuario);

        // STEP 4: Rota Protegida com Token Válido
        await Passo_AcessarRotaProtegida(tokensIniciais.AccessToken, true);

        // STEP 5: Refresh Token (Rotação de Credenciais)
        var novosTokens = await Passo_RealizarRefreshToken(tokensIniciais.RefreshToken);

        // STEP 6: Valida se o novo token funciona
        await Passo_AcessarRotaProtegida(novosTokens.AccessToken, true);

        // STEP 7: Tentar Refresh com Token Inválido/Adulterado
        await Passo_BloquearRefreshTokenInvalido();

        // STEP 8: Logout (Revogação)
        await Passo_RealizarLogout(novosTokens);

        // STEP 9: Tentar usar tokens após Logout
        await Passo_AcessarRotaProtegida(novosTokens.AccessToken, false);
    }

    #region Steps (Passos do Roteiro)

    private async Task Passo_TentarLoginUsuarioInexistente()
    {
        var payload = new { Username = "Fantasma", Password = "123" };
        var response = await _client.PostAsJsonAsync(EndpointLogin, payload);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "tentativa de login de usuário não cadastrado deve ser negada como Unauthorized");
    }

    private async Task Passo_RegistrarUsuario(object payload)
    {
        var response = await _client.PostAsJsonAsync(EndpointRegister, payload);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    private async Task<TokenResponseDto> Passo_RealizarLoginComSucesso(object payload)
    {
        var response = await _client.PostAsJsonAsync(EndpointLogin, payload);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<TokenResponseDto>();

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.AccessToken.Should().NotBeNullOrEmpty("o login deve retornar um Access Token JWT");
            result.RefreshToken.Should().NotBeNullOrEmpty("o login deve retornar um Refresh Token");
        }

        return result!;
    }

    private async Task Passo_AcessarRotaProtegida(string accessToken, bool deveAutorizar)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, EndpointProfile);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        if (deveAutorizar)
            response.StatusCode.Should().Be(HttpStatusCode.OK,
                "um token válido e ativo deve permitir acesso a rotas protegidas");
        else
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                "um token revogado (após logout) ou inválido deve ser bloqueado pelo Middleware");
    }

    private async Task<TokenResponseDto> Passo_RealizarRefreshToken(string refreshTokenAtual)
    {
        var payload = new { refreshToken = refreshTokenAtual };

        var response = await _client.PostAsJsonAsync(EndpointRefreshToken, payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "um refresh token válido deve gerar novas credenciais");

        var result = await response.Content.ReadFromJsonAsync<TokenResponseDto>();

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
        }

        return result!;
    }

    private async Task Passo_BloquearRefreshTokenInvalido()
    {
        var payload = new { refreshToken = "TokenTotalmenteFalsoOuExpirado123" };
        var response = await _client.PostAsJsonAsync(EndpointRefreshToken, payload);

        response.StatusCode.Should().BeOneOf(
            [HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized],
            "tentar renovar sessão com token falso deve falhar");
    }

    private async Task Passo_RealizarLogout(TokenResponseDto novosTokens)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", novosTokens.AccessToken);
        var payload = new { refreshToken = novosTokens.RefreshToken };

        var response = await _client.PostAsJsonAsync(EndpointLogout, payload);

        response.StatusCode.Should().BeOneOf(
            [HttpStatusCode.OK, HttpStatusCode.NoContent],
            "o logout deve ser processado com sucesso");
    }

    #endregion
}