using BeachApplication.MultiTenant.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BeachApplication.MultiTenant.Extensions;

public static class MultiTenantServiceExtensions
{
    public static IServiceCollection AddMultiTenant(this IServiceCollection services, Action<TenantContextOptions> configuration)
    {
        var options = new TenantContextOptions();
        configuration.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<ITenantContextAccessor, TenantContextAccessor>();

        services.AddSingleton<ITenantService, TenantService>();
        services.AddSingleton<IAuthorizationHandler, ValidateTenantHandler>();

        return services;
    }

    public static IApplicationBuilder UseMultiTenant(this IApplicationBuilder app)
    {
        if (app.ApplicationServices.GetService(typeof(ITenantContextAccessor)) is null)
        {
            throw new ApplicationException("Unable to find the required service");
        }

        app.UseMiddleware<TenantContextMiddleware>();
        return app;
    }
}