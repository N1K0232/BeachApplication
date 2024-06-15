using System.Security.Claims;
using BeachApplication.Authentication.Entities;
using BeachApplication.Authentication.Extensions;
using BeachApplication.Authentication.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BeachApplication.Authentication.Handlers;

public class UserActiveHandler : AuthorizationHandler<UserActiveRequirement>
{
    private readonly UserManager<ApplicationUser> userManager;

    public UserActiveHandler(UserManager<ApplicationUser> userManager)
    {
        this.userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UserActiveRequirement requirement)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            var user = await userManager.FindByNameAsync(context.User.GetUserName());
            var lockedOut = await userManager.IsLockedOutAsync(user);
            var securityStamp = context.User.GetClaimValue(ClaimTypes.SerialNumber);

            if (user is not null && !lockedOut && securityStamp == user.SecurityStamp)
            {
                context.Succeed(requirement);
            }
        }
    }
}