namespace BeachApplication.MultiTenant;

public abstract class TenantContext
{
    public abstract string Name { get; set; }
}