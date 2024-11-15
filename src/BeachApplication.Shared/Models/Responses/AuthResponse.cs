namespace BeachApplication.Shared.Models.Responses;

public class AuthResponse
{
    public AuthResponse(string token)
    {
        Token = token;
    }

    public AuthResponse(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }

    public string? Token { get; }

    public string? AccessToken { get; }

    public string? RefreshToken { get; }
}