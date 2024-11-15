using Microsoft.IdentityModel.Tokens;

namespace BeachApplication.Authentication.JwtBearer;

public class JwtBearerSettings
{
    public string SecurityKey { get; init; }

    public string Issuer { get; init; }

    public string Audience { get; init; }

    public string SecurityAlgorithm { get; init; } = SecurityAlgorithms.HmacSha256;

    public TimeSpan AccessTokenExpirationTime { get; init; }

    public TimeSpan RefreshTokenExpirationTime { get; init; }

    public TimeSpan ClockSkew { get; init; }
}