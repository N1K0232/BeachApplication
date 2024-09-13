using BeachApplication.DataAccessLayer;
using BeachApplication.DataAccessLayer.Entities.Identity;
using BeachApplication.DataAccessLayer.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BeachApplication.BusinessLayer.StartupServices;

public class IdentityUserService(IServiceProvider services, IConfiguration configuration) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var administratorUserSection = configuration.GetSection("AdministratorUser");
        var powerUserSection = configuration.GetSection("PowerUser");

        var administratorUser = new ApplicationUser
        {
            FirstName = administratorUserSection["FirstName"],
            Email = administratorUserSection["Email"],
            UserName = administratorUserSection["Email"]
        };

        var powerUser = new ApplicationUser
        {
            FirstName = powerUserSection["FirstName"],
            Email = powerUserSection["Email"],
            UserName = powerUserSection["Email"]
        };

        var administratorUserPassword = administratorUserSection["Password"];
        var powerUserPassword = powerUserSection["Password"];

        await CreateAsync(administratorUser, administratorUserPassword, [RoleNames.Administrator, RoleNames.User]);
        await CreateAsync(powerUser, powerUserPassword, [RoleNames.PowerUser, RoleNames.User]);
    }

    private async Task CreateAsync(ApplicationUser user, string password, IEnumerable<string> roles)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var userExists = await userManager.UserExistsAsync(user.UserName);
        if (!userExists)
        {
            await userManager.CreateAsync(user, password);
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            await userManager.ConfirmEmailAsync(user, token);
            await userManager.AddToRolesAsync(user, roles);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}