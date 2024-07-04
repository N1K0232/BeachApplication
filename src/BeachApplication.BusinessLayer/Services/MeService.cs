using BeachApplication.Authentication.Entities;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Contracts;
using BeachApplication.Shared.Models;
using Microsoft.AspNetCore.Identity;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services;

public class MeService : IMeService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IUserService userService;

    public MeService(UserManager<ApplicationUser> userManager, IUserService userService)
    {
        this.userManager = userManager;
        this.userService = userService;
    }

    public async Task<Result<User>> GetAsync()
    {
        var userName = await userService.GetUserNameAsync();
        var user = await userManager.FindByNameAsync(userName);

        return new User(user.Id, user.FirstName, user.LastName, user.Email);
    }
}