using System.Security.Claims;

namespace BatalhaNaval.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier);

        if (claim == null)
            throw new UnauthorizedAccessException("Token inválido: ID do usuário não encontrado nas claims.");

        if (!Guid.TryParse(claim.Value, out var userId))
            throw new UnauthorizedAccessException("Token inválido: ID do usuário não é um GUID válido.");

        return userId;
    }
}