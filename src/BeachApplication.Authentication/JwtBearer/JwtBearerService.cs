using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace BeachApplication.Authentication.JwtBearer;

public class JwtBearerService(IOptions<JwtBearerSettings> jwtBearerSettingsOptions) : IJwtBearerService
{
    public async Task<string> CreateTokenAsync(string userName, IList<Claim> claims)
    {
        var hostName = Dns.GetHostName();
        var addresses = await Dns.GetHostAddressesAsync(hostName);

        claims.Add(new Claim(JwtRegisteredClaimNames.UniqueName, userName));
        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

        claims.Add(new Claim(ClaimTypes.Dns, hostName));
        foreach (var address in addresses)
        {
            claims.Add(new Claim(ClaimTypes.Dns, address.ToString()));
        }

        var now = DateTime.UtcNow;
        var notBefore = now.Add(-jwtBearerSettingsOptions.Value.ClockSkew);

        var expires = now.Add(jwtBearerSettingsOptions.Value.AccessTokenExpirationTime);
        var securityKey = Encoding.UTF8.GetBytes(jwtBearerSettingsOptions.Value.SecurityKey);

        var symmetricSecurityKey = new SymmetricSecurityKey(securityKey);
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, jwtBearerSettingsOptions.Value.SecurityAlgorithm);

        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims, "Bearer", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType),
            Issuer = jwtBearerSettingsOptions.Value.Issuer,
            Audience = jwtBearerSettingsOptions.Value.Audience,
            IssuedAt = now,
            NotBefore = notBefore,
            Expires = expires,
            SigningCredentials = signingCredentials
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(securityTokenDescriptor);
    }

    public async Task<ClaimsPrincipal> ValidateTokenAsync(string accessToken, bool validateLifetime = false)
    {
        var tokenHandler = new JsonWebTokenHandler();

        if (!tokenHandler.CanReadToken(accessToken))
        {
            throw new SecurityTokenException("Token is not a well formed Json Web Token (JWT)");
        }

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtBearerSettingsOptions.Value.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtBearerSettingsOptions.Value.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtBearerSettingsOptions.Value.SecurityKey)),
            RequireExpirationTime = true,
            ClockSkew = jwtBearerSettingsOptions.Value.ClockSkew
        };

        var validationResult = await tokenHandler.ValidateTokenAsync(accessToken, tokenValidationParameters);
        if (!validationResult.IsValid || validationResult.SecurityToken is not JsonWebToken jsonWebToken || jsonWebToken.Alg != SecurityAlgorithms.HmacSha256)
        {
            throw new SecurityTokenException("Token is expired or invalid", validationResult.Exception);
        }

        return new ClaimsPrincipal(validationResult.ClaimsIdentity);
    }
}