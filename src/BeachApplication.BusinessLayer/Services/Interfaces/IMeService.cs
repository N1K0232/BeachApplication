using System.Security.Claims;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services.Interfaces;

public interface IMeService
{
    Task<Result> ChangePhoneNumberAsync(ClaimsPrincipal principal, ChangePhoneNumberRequest request);

    Task<Result> EnableTwoFactorAsync(ClaimsPrincipal principal);

    Task<Result<User>> GetAsync(ClaimsPrincipal principal);
}