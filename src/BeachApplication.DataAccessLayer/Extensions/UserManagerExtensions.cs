using BeachApplication.DataAccessLayer.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace BeachApplication.DataAccessLayer.Extensions;

public static class UserManagerExtensions
{
    public static async Task<bool> UserExistsAsync(this UserManager<ApplicationUser> userManager, string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return false;
        }

        var user = await userManager.FindByNameAsync(userName);
        return user is not null;
    }

    public static async Task<bool> AuthenticatorKeyExistsAsync(this UserManager<ApplicationUser> userManager, ApplicationUser user)
    {
        var secret = await userManager.GetAuthenticatorKeyAsync(user);
        return !string.IsNullOrWhiteSpace(secret);
    }
}