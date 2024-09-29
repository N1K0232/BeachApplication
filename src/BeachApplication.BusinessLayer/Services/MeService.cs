using System.Security.Claims;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.DataAccessLayer.Extensions;
using BeachApplication.Shared.Models;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services;

public class MeService : IMeService
{
    public Task<Result<User>> GetAsync(ClaimsPrincipal principal)
    {
        var user = new User
        {
            Id = principal.GetId(),
            FirstName = principal.GetClaimValue(ClaimTypes.GivenName),
            LastName = principal.GetClaimValue(ClaimTypes.Surname),
            Email = principal.GetEmail()
        };

        var result = Result<User>.Ok(user);
        return Task.FromResult(result);
    }
}