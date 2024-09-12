using BeachApplication.Authentication.Entities;
using Microsoft.AspNetCore.Identity;

namespace BeachApplication.Authentication.Extensions;

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
}