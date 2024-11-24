namespace BeachApplication.MultiTenant;

public class TenantContextOptions
{
    public IList<string> AvailableTenants { get; set; } = [];
}