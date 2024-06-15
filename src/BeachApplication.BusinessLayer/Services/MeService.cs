using BeachApplication.Authentication.Entities;
using BeachApplication.Authentication.Extensions;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services;

public class MeService : IMeService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IHttpContextAccessor httpContextAccessor;

    public MeService(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
    {
        this.userManager = userManager;
        this.httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<User>> GetAsync()
    {
        var userName = httpContextAccessor.HttpContext.User.GetUserName();
        var user = await userManager.FindByNameAsync(userName);

        return new User(user.FirstName, user.LastName, user.Email);
    }
}