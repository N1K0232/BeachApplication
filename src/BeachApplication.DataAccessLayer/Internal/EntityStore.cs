using BeachApplication.DataAccessLayer.Entities.Common;
using BeachApplication.DataProtectionLayer.Services;

namespace BeachApplication.DataAccessLayer.Internal;

public class EntityStore(IDataProtectionService dataProtectionService) : IEntityStore
{
    public Task GenerateConcurrencyStampAsync<T>(T entity) where T : BaseEntity
    {
        entity.ConcurrencyStamp = Guid.NewGuid().ToString();
        return Task.CompletedTask;
    }

    public async Task GenerateSecurityStampAsync<T>(T entity) where T : BaseEntity
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        var result = new char[50];
        var random = new Random();

        for (var i = 0; i < result.Length; i++)
        {
            result[i] = chars[random.Next(chars.Length)];
        }

        var securityStamp = new string(result);
        entity.SecurityStamp = await dataProtectionService.ProtectAsync(securityStamp);
    }
}