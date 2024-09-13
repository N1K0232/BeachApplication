using Microsoft.AspNetCore.DataProtection;

namespace BeachApplication.DataAccessLayer.DataProtection;

public class DataProtectionService(IDataProtector dataProtector) : IDataProtectionService
{
    public Task<string> ProtectAsync(string input, CancellationToken cancellationToken = default)
    {
        var result = dataProtector.Protect(input);
        return Task.FromResult(result);
    }

    public Task<string> UnprotectAsync(string input, CancellationToken cancellationToken = default)
    {
        var result = dataProtector.Unprotect(input);
        return Task.FromResult(result);
    }
}