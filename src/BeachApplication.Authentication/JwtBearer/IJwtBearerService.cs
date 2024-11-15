using System.Security.Claims;

namespace BeachApplication.Authentication.JwtBearer;

public interface IJwtBearerService
{
    Task<string> CreateTokenAsync(string userName, IList<Claim> claims);

    Task<ClaimsPrincipal> ValidateTokenAsync(string accessToken, bool validateLifetime = false);
}