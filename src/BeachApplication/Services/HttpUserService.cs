using BeachApplication.Authentication.Extensions;
using BeachApplication.Contracts;

namespace BeachApplication.Services;

public class HttpUserService(IHttpContextAccessor httpContextAccessor) : IUserService
{
    public Task<Guid> GetIdAsync()
    {
        var userId = httpContextAccessor.HttpContext?.User.GetId();
        return Task.FromResult(userId ?? Guid.Empty);
    }

    public Task<string> GetUserNameAsync()
    {
        var userName = httpContextAccessor.HttpContext?.User.GetUserName();
        return Task.FromResult(userName ?? string.Empty);
    }
}