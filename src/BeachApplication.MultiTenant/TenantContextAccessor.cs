namespace BeachApplication.MultiTenant;

internal class TenantContextAccessor : ITenantContextAccessor
{
    private static readonly AsyncLocal<TenantContext> tenantContext = new();

    public TenantContext TenantContext
    {
        get => tenantContext.Value;
        set => tenantContext.Value = value;
    }
}