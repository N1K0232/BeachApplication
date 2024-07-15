
namespace BeachApplication.DataProtectionLayer.Services;

public interface IDataProtectionService
{
    Task<string> ProtectAsync(string input, CancellationToken cancellationToken = default);

    Task<string> UnprotectAsync(string input, CancellationToken cancellationToken = default);
}