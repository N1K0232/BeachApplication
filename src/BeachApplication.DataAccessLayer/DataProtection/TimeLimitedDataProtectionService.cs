using Microsoft.AspNetCore.DataProtection;

namespace BeachApplication.DataAccessLayer.DataProtection;

public class TimeLimitedDataProtectionService(ITimeLimitedDataProtector dataProtector) : ITimeLimitedDataProtectionService
{
    public Task<string> ProtectAsync(string input, TimeSpan lifetime, CancellationToken cancellationToken = default)
    {
        var result = dataProtector.Protect(input, lifetime);
        return Task.FromResult(result);
    }

    public Task<string> UnprotectAsync(string input, CancellationToken cancellationToken = default)
    {
        var result = dataProtector.Unprotect(input);
        return Task.FromResult(result);
    }
}