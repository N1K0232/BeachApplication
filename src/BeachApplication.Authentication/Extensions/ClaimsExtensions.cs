using System.Security.Claims;
using System.Security.Principal;

namespace BeachApplication.Authentication.Extensions;

public static class ClaimsExtensions
{
    public static Guid GetId(this IPrincipal user)
    {
        var value = GetClaimValueInternal(user, ClaimTypes.NameIdentifier);
        if (Guid.TryParse(value, out var userId))
        {
            return userId;
        }

        return Guid.Empty;
    }

    public static string? GetUserName(this IPrincipal user)
        => GetClaimValueInternal(user, ClaimTypes.Name);

    public static string? GetEmail(this IPrincipal user)
        => GetClaimValueInternal(user, ClaimTypes.Email);

    public static string? GetClaimValue(this IPrincipal user, string claimType)
        => GetClaimValueInternal(user, claimType);

    internal static string? GetClaimValueInternal(this IPrincipal user, string claimType)
        => ((ClaimsPrincipal)user).FindFirstValue(claimType);
}