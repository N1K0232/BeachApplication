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
        claims.Union(addresses.Select(address => new Claim(ClaimTypes.Dns, address.ToString())));

        var now = DateTime.UtcNow;
        var notBefore = now.Add(-jwtBearerSettingsOptions.Value.ClockSkew);

        var expires = now.Add(jwtBearerSettingsOptions.Value.ExpirationTime);
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
}
