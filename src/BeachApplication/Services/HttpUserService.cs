using System.Security.Claims;
using BeachApplication.Authentication.Entities;
using BeachApplication.Authentication.Extensions;
using BeachApplication.Contracts;
using Microsoft.AspNetCore.Identity;

namespace BeachApplication.Services;

public class HttpUserService(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor) : IUserService
{
    public ClaimsIdentity GetIdentity()
    {
        var identity = httpContextAccessor.HttpContext.User.Identity;
        return identity as ClaimsIdentity;
    }

    public Guid GetTenantId()
    {
        string tenantHeader = httpContextAccessor.HttpContext.Request.Headers["TenantId"];
        if (Guid.TryParse(tenantHeader, out var tenantId))
        {
            return tenantId;
        }

        return Guid.Empty;
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