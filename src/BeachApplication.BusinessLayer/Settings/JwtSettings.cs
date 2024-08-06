namespace BeachApplication.BusinessLayer.Settings;

public class JwtSettings
{
    public string SecurityKey { get; init; } = null!;

    public string Issuer { get; init; } = null!;

    public string Audience { get; init; } = null!;

    public int AccessTokenExpirationMinutes { get; init; }

    public int RefreshTokenExpirationMinutes { get; init; }
}