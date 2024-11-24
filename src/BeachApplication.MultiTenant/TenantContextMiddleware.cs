using Microsoft.AspNetCore.Http;

namespace BeachApplication.MultiTenant;

internal class TenantContextMiddleware : IMiddleware
{
    private readonly ITenantContextAccessor tenantContextAccessor;
    private readonly TenantContextOptions options;

    public TenantContextMiddleware(ITenantContextAccessor tenantContextAccessor, TenantContextOptions options)
    {
        this.tenantContextAccessor = tenantContextAccessor;
        this.options = options;
    }

    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        tenantContextAccessor.TenantContext = new DefaultTenantContext();

        var host = httpContext.Request.Host.Host;
        var tenant = host?.Split('.').ElementAtOrDefault(0)?.Trim().ToLowerInvariant();

        if (options.AvailableTenants.Contains(tenant))
        {
            tenantContextAccessor.TenantContext.Name = tenant;
            await next.Invoke(httpContext);
            return;
        }

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
    }
}