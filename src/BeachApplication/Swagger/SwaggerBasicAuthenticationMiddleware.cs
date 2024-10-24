using System.Net.Http.Headers;
using System.Text;
using BeachApplication.BusinessLayer.Settings;
using BeachApplication.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using TinyHelpers.Extensions;

namespace BeachApplication.Swagger;

public class SwaggerBasicAuthenticationMiddleware(RequestDelegate next, IOptions<SwaggerSettings> swaggerSettingsOptions)
{
    private readonly SwaggerSettings swaggerSettings = swaggerSettingsOptions.Value;

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (httpContext.IsSwaggerRequest() && swaggerSettings.UserName.HasValue() && swaggerSettings.Password.HasValue())
        {
            string authenticationHeader = httpContext.Request.Headers[HeaderNames.Authorization];
            if (authenticationHeader?.StartsWith("Basic ") ?? false)
            {
                var (userName, password) = GetCredentials(authenticationHeader);
                if (userName == swaggerSettings.UserName && password == swaggerSettings.Password)
                {
                    await next.Invoke(httpContext);
                    return;
                }
            }

            httpContext.Response.Headers.WWWAuthenticate = new StringValues("Basic");
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        else
        {
            await next.Invoke(httpContext);
        }
    }

    private static (string UserName, string Password) GetCredentials(string authenticationHeader)
    {
        var header = AuthenticationHeaderValue.Parse(authenticationHeader);
        var parameter = Convert.FromBase64String(header.Parameter);
        var credentials = Encoding.UTF8.GetString(parameter).Split(':', count: 2);

        return (credentials.ElementAtOrDefault(0), credentials.ElementAtOrDefault(1));
    }
}