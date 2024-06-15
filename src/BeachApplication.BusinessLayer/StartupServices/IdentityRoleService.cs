using BeachApplication.Authentication;
using BeachApplication.Authentication.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BeachApplication.BusinessLayer.StartupServices;

public class IdentityRoleService : IHostedService
{
    private readonly IServiceProvider services;

    public IdentityRoleService(IServiceProvider services)
    {
        this.services = services;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        var roleNames = new string[] { RoleNames.Administrator, RoleNames.PowerUser, RoleNames.User };
        foreach (var roleName in roleNames)
        {
            var exists = await roleManager.RoleExistsAsync(roleName);
            if (!exists)
            {
                var role = new ApplicationRole(roleName)
                {
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };

                await roleManager.CreateAsync(role);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}