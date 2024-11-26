namespace BeachApplication.Authentication.Entities;

public class Tenant
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string SqlConnectionString { get; set; }

    public string AzureStorageConnectionString { get; set; }

    public string ContainerName { get; set; }

    public virtual ICollection<ApplicationUser> Users { get; set; }
}