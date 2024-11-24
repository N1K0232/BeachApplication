using Microsoft.AspNetCore.Authorization;

namespace BeachApplication.MultiTenant.Authorization;

public class ValidateTenantHandler : AuthorizationHandler<TenantRequirement>
{
    private readonly ITenantService tenantService;

    public ValidateTenantHandler(ITenantService tenantService)
    {
        this.tenantService = tenantService;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TenantRequirement requirement)
    {
        var user = context.User;
        if (user.Identity.IsAuthenticated)
        {
            var tenant = tenantService.GetCurrent();
            var claim = user.Claims.FirstOrDefault(c => c.Type == "aud");

            if (claim.Value == tenant.Name)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}