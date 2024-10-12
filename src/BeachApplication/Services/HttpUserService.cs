using System.Security.Claims;
using BeachApplication.Contracts;
using SimpleAuthentication;

namespace BeachApplication.Services;

public class HttpUserService(IHttpContextAccessor httpContextAccessor) : IUserService
{
    public Task<Guid> GetIdAsync()
    {
        var value = httpContextAccessor.HttpContext.User.GetClaimValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(value, out var userId))
        {
            return Task.FromResult(userId);
        }

        return Task.FromResult(Guid.Empty);
    }

    public Task<string> GetUserNameAsync()
    {
        var userName = httpContextAccessor.HttpContext.User.Identity.Name;
        return Task.FromResult(userName);
    }
}