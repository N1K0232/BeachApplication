namespace BeachApplication.Contracts;

public interface IUrlGeneratorService
{
    Task<string?> GetPageUrlAsync(string page, object? values = null);
}