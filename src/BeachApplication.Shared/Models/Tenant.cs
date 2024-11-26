namespace BeachApplication.Shared.Models;

public record class Tenant(Guid Id, string Name, string SqlConnectionString, string? AzureStorageConnectionString, string? ContainerName);