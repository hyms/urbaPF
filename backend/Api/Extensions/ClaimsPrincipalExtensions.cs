using System.Security.Claims;
using UrbaPF.Domain.Enums;

namespace UrbaPF.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim != null ? Guid.Parse(userIdClaim) : Guid.Empty;
    }

    public static UserRole GetUserRole(this ClaimsPrincipal user)
    {
        var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
        return roleClaim != null ? (UserRole)int.Parse(roleClaim) : UserRole.Restricted;
    }

    public static Guid GetCondominiumId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst("condominiumId")?.Value;
        return claim != null ? Guid.Parse(claim) : Guid.Empty;
    }
}