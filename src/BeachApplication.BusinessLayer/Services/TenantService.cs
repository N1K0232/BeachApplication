using BeachApplication.Authentication;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Contracts;
using BeachApplication.Shared.Models;

namespace BeachApplication.BusinessLayer.Services;

public class TenantService(AuthenticationDbContext authenticationDbContext, IUserService userService) : ITenantService
{
    public Tenant Get()
    {
        var tenants = authenticationDbContext.Tenants
            .ToDictionary(k => k.Id, v => new Tenant(v.Id, v.Name, v.SqlConnectionString, v.AzureStorageConnectionString, v.ContainerName));

        var tenantId = userService.GetTenantId();
        if (tenants.TryGetValue(tenantId, out var tenant))
        {
            return tenant;
        }

        return null;
    }
}