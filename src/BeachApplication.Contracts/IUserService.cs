using System.Security.Claims;

namespace BeachApplication.Contracts;

public interface IUserService
{
    ClaimsIdentity GetIdentity();

    Guid GetTenantId();

    Guid GetUserId();

    string GetUserName();
}