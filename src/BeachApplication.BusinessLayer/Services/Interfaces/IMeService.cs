using System.Security.Claims;
using BeachApplication.Shared.Models;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services.Interfaces;

public interface IMeService
{
    Task<Result> EnableTwoFactorAsync(ClaimsPrincipal principal);

    Task<Result<User>> GetAsync(ClaimsPrincipal principal);
}