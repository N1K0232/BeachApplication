namespace BeachApplication.MultiTenant;

public interface ITenantContextAccessor
{
    TenantContext TenantContext { get; internal set; }
}