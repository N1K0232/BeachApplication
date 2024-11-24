using Microsoft.Extensions.Options;

namespace BeachApplication.MultiTenant;

internal class TenantService(ITenantContextAccessor tenantContextAccessor, IOptions<List<Tenant>> tenantOptions) : ITenantService
{
    private readonly List<Tenant> tenants = tenantOptions.Value;

    public Tenant GetCurrent()
    {
        var context = tenantContextAccessor.TenantContext;
        return tenants.FirstOrDefault(t => t.Name == context.Name);
    }
}