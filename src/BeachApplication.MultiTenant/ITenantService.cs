namespace BeachApplication.MultiTenant;

public interface ITenantService
{
    Tenant GetCurrent();
}