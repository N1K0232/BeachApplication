using BeachApplication.Authentication;
using BeachApplication.Authentication.Entities;
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
            FirstName = administratorUserSection["FirstName"]!,
            Email = administratorUserSection["Email"],
            UserName = administratorUserSection["Email"]
        };

        var powerUser = new ApplicationUser
        {
            FirstName = powerUserSection["FirstName"]!,
            Email = powerUserSection["Email"],
            UserName = powerUserSection["Email"]
        };

        await CreateAsync(administratorUser, administratorUserSection["Password"]!, RoleNames.Administrator, RoleNames.User);
        await CreateAsync(powerUser, powerUserSection["Password"]!, RoleNames.PowerUser, RoleNames.User);
    }

    private async Task CreateAsync(ApplicationUser user, string password, params string[] roles)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRolesAsync(user, roles);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}