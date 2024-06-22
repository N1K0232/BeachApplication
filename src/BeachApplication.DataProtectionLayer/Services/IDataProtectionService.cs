
namespace BeachApplication.DataProtectionLayer.Services;

public interface IDataProtectionService
{
    Task<string> ProtectAsync(string input);
    Task<string> UnprotectAsync(string input);
}