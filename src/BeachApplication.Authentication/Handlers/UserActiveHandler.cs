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
        var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
        if (isAuthenticated)
        {
            var userName = context.User.GetClaimValueInternal(ClaimTypes.Name);
            if (!string.IsNullOrWhiteSpace(userName))
            {
                var user = await userManager.FindByNameAsync(userName);
                if (user is not null)
                {
                    var lockedOut = await userManager.IsLockedOutAsync(user);
                    var securityStamp = context.User.GetClaimValueInternal(ClaimTypes.SerialNumber);

                    if (!lockedOut && securityStamp == user.SecurityStamp)
                    {
                        context.Succeed(requirement);
                    }
                }
            }
        }
    }
}