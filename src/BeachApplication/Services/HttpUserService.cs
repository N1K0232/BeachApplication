using BeachApplication.Authentication.Extensions;
using BeachApplication.Contracts;

namespace BeachApplication.Services;

public class HttpUserService : IUserService
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public HttpUserService(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public Task<Guid> GetIdAsync()
    {
        var userId = httpContextAccessor.HttpContext.User.GetId();
        return Task.FromResult(userId);
    }

    public Task<string> GetUserNameAsync()
    {
        var userName = httpContextAccessor.HttpContext.User.GetUserName();
        return Task.FromResult(userName);
    }
}