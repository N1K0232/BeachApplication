using System.Security.Claims;
using System.Security.Principal;

namespace BeachApplication.Authentication.Extensions;

public static class ClaimsExtensions
{
    public static string GetClaimValue(this IPrincipal user, string claimType)
    {
        var value = ((ClaimsPrincipal)user).FindFirstValue(claimType);
        return value;
    }
}