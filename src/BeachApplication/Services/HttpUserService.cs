using System.Security.Claims;
using BeachApplication.Authentication.Extensions;
using BeachApplication.Contracts;

namespace BeachApplication.Services;

public class HttpUserService(IHttpContextAccessor httpContextAccessor) : IUserService
{
    public ClaimsIdentity GetIdentity()
    {
        var identity = httpContextAccessor.HttpContext.User.Identity;
        return identity as ClaimsIdentity;
    }

    public Guid GetUserId()
    {
        var value = httpContextAccessor.HttpContext.User.GetClaimValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(value, out var userId))
        {
            return userId;
        }

        return Guid.Empty;
    }

    public string GetUserName() => httpContextAccessor.HttpContext.User.Identity.Name;
}