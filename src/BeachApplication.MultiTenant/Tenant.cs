namespace BeachApplication.MultiTenant;

public class Tenant
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string SqlConnectionString { get; set; }

    public string AzureStorageConnectionString { get; set; }

    public string ContainerName { get; set; }
}