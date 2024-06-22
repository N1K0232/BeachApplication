using Microsoft.AspNetCore.DataProtection;

namespace BeachApplication.DataProtectionLayer.Services;

public class DataProtectionService(IDataProtector protector) : IDataProtectionService
{
    public Task<string> ProtectAsync(string input)
    {
        var result = protector.Protect(input);
        return Task.FromResult(result);
    }

    public Task<string> UnprotectAsync(string input)
    {
        var result = protector.Unprotect(input);
        return Task.FromResult(result);
    }
}