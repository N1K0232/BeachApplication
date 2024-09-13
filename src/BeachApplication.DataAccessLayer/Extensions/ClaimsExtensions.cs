using System.Security.Claims;
using System.Security.Principal;

namespace BeachApplication.DataAccessLayer.Extensions;

public static class ClaimsExtensions
{
    public static Guid GetId(this IPrincipal user)
    {
        var value = user.GetClaimValueInternal(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(value, out var userId))
        {
            return userId;
        }

        return Guid.Empty;
    }

    public static string GetUserName(this IPrincipal user)
        => user.GetClaimValueInternal(ClaimTypes.Name);

    public static string GetEmail(this IPrincipal user)
        => user.GetClaimValueInternal(ClaimTypes.Email);

    public static string GetClaimValue(this IPrincipal user, string claimType)
        => user.GetClaimValueInternal(claimType);

    internal static string GetClaimValueInternal(this IPrincipal user, string claimType)
        => ((ClaimsPrincipal)user).FindFirstValue(claimType);
}