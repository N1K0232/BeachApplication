
namespace BeachApplication.DataAccessLayer.DataProtection;

public interface ITimeLimitedDataProtectionService
{
    Task<string> ProtectAsync(string input, TimeSpan lifetime, CancellationToken cancellationToken = default);

    Task<string> UnprotectAsync(string input, CancellationToken cancellationToken = default);
}