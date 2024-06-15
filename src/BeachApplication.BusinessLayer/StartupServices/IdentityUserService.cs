using BeachApplication.Authentication;
using BeachApplication.Authentication.Entities;
using BeachApplication.BusinessLayer.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BeachApplication.BusinessLayer.StartupServices;

public class IdentityUserService : IHostedService
{
    private readonly IServiceProvider services;
    private readonly AdministratorUserSettings administratorUserSettings;
    private readonly PowerUserSettings powerUserSettings;

    public IdentityUserService(IServiceProvider services,
        IOptions<AdministratorUserSettings> admistratorUserSettingsOptions,
        IOptions<PowerUserSettings> powerUserSettingsOptions)
    {
        this.services = services;
        administratorUserSettings = admistratorUserSettingsOptions.Value;
        powerUserSettings = powerUserSettingsOptions.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var administratorUser = new ApplicationUser
        {
            FirstName = administratorUserSettings.FirstName,
            Email = administratorUserSettings.Email,
            UserName = administratorUserSettings.Email
        };

        var powerUser = new ApplicationUser
        {
            FirstName = powerUserSettings.FirstName,
            Email = powerUserSettings.Email,
            UserName = powerUserSettings.Email
        };

        await CreateAsync(administratorUser, administratorUserSettings.Password, RoleNames.Administrator, RoleNames.User);
        await CreateAsync(powerUser, powerUserSettings.Password, RoleNames.PowerUser, RoleNames.User);
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