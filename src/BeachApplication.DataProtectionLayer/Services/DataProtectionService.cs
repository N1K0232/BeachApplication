using Microsoft.AspNetCore.DataProtection;

namespace BeachApplication.DataProtectionLayer.Services;

public class DataProtectionService(IDataProtector protector) : IDataProtectionService
{
    public Task<string> ProtectAsync(string input, CancellationToken cancellationToken = default)
    {
        var result = protector.Protect(input);
        return Task.FromResult(result);
    }

    public Task<string> UnprotectAsync(string input, CancellationToken cancellationToken = default)
    {
        var result = protector.Unprotect(input);
        return Task.FromResult(result);
    }
}