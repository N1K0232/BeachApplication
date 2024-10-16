using Microsoft.AspNetCore.DataProtection;

namespace BeachApplication.DataAccessLayer.DataProtection;

public class DataProtectionService(ITimeLimitedDataProtector protector) : IDataProtectionService
{
    public Task<string> ProtectAsync(string plaintext, TimeSpan lifetime, CancellationToken cancellationToken = default)
    {
        var result = protector.Protect(plaintext, lifetime);
        return Task.FromResult(result);
    }

    public Task<string> UnprotectAsync(string protectedData, CancellationToken cancellationToken = default)
    {
        var result = protector.Unprotect(protectedData);
        return Task.FromResult(result);
    }
}