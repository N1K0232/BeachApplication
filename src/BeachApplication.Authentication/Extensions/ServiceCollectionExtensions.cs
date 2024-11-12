using System.Text;
using BeachApplication.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BeachApplication.Authentication.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration, string sectionName = nameof(JwtBearerSettings))
    {
        var section = configuration.GetSection(sectionName);
        var settings = section.Get<JwtBearerSettings>();

        services.Configure<JwtBearerSettings>(section);
        services.AddSingleton<IJwtBearerService, JwtBearerService>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = settings.ExpirationTime;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = settings.Issuer,
                ValidateAudience = true,
                ValidAudience = settings.Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecurityKey)),
                RequireExpirationTime = true,
                ClockSkew = settings.ClockSkew
            };
        });

        return services;
    }
}