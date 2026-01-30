using System.Security.Claims;
using BatalhaNaval.Domain.Entities;

namespace BatalhaNaval.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);

    string GenerateRefreshToken();

    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}