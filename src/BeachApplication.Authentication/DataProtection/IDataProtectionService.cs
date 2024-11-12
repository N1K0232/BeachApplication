
namespace BeachApplication.Authentication.DataProtection;

public interface IDataProtectionService
{
    Task<string> ProtectAsync(string plaintext, TimeSpan lifetime, CancellationToken cancellationToken = default);

    Task<string> UnprotectAsync(string protectedData, CancellationToken cancellationToken = default);
}