using System.Security.Claims;
using BeachApplication.Authentication.Extensions;
using BeachApplication.Contracts;
using BeachApplication.MultiTenant;

namespace BeachApplication.Services;

public class HttpUserService : IUserService
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ITenantService tenantService;

    public HttpUserService(IHttpContextAccessor httpContextAccessor, ITenantService tenantService)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.tenantService = tenantService;
    }

    public ClaimsIdentity GetIdentity()
    {
        var identity = httpContextAccessor.HttpContext.User.Identity;
        return identity as ClaimsIdentity;
    }

    public Guid GetTenantId()
    {
        var tenant = tenantService.GetCurrent();
        return tenant.Id;
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