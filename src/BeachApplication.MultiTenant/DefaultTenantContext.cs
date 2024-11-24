namespace BeachApplication.MultiTenant;

public class DefaultTenantContext : TenantContext
{
    private string name;

    public override string Name
    {
        get
        {
            return name;
        }
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

            if (value != name)
            {
                name = value;
            }
        }
    }
}