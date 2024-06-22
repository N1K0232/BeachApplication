using System.Security.Claims;
using BeachApplication.Authentication.Entities;
using BeachApplication.Authentication.Extensions;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Shared.Models;
using Microsoft.AspNetCore.Identity;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services;

public class MeService : IMeService
{
    private readonly UserManager<ApplicationUser> userManager;

    public MeService(UserManager<ApplicationUser> userManager)
    {
        this.userManager = userManager;
    }

    public async Task<Result<User>> GetAsync(ClaimsPrincipal principal)
    {
        var user = await userManager.FindByNameAsync(principal.GetUserName());
        return new User(user.Id, user.FirstName, user.LastName, user.Email);
    }
}