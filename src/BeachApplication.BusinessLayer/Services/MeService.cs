using System.Security.Claims;
using AutoMapper;
using BeachApplication.Authentication.Entities;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Microsoft.AspNetCore.Identity;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services;

public class MeService(UserManager<ApplicationUser> userManager, IMapper mapper) : IMeService
{
    public async Task<Result> ChangePhoneNumberAsync(ClaimsPrincipal principal, ChangePhoneNumberRequest request)
    {
        await Task.Delay(50);
        return Result.Ok();
    }

    public async Task<Result> EnableTwoFactorAsync(ClaimsPrincipal principal)
    {
        var user = await userManager.GetUserAsync(principal);
        user.TwoFactorEnabled = true;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var detail = string.Join(',', result.Errors.Select(e => e.Description));
            return Result.Fail(FailureReasons.ClientError, "Couldn't update the profile", detail);
        }

        return Result.Ok();
    }

    public async Task<Result<User>> GetAsync(ClaimsPrincipal principal)
    {
        var user = await userManager.GetUserAsync(principal);
        return mapper.Map<User>(user);
    }
}