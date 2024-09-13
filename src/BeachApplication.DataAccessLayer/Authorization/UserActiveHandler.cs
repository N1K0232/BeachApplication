using System.Security.Claims;
using BeachApplication.DataAccessLayer.Entities.Identity;
using BeachApplication.DataAccessLayer.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BeachApplication.DataAccessLayer.Authorization;

public class UserActiveHandler(UserManager<ApplicationUser> userManager) : AuthorizationHandler<UserActiveRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UserActiveRequirement requirement)
    {
        var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
        if (isAuthenticated)
        {
            var userName = context.User.GetClaimValueInternal(ClaimTypes.Name);
            var user = await userManager.FindByNameAsync(userName);

            var lockedOut = await userManager.IsLockedOutAsync(user);
            var securityStamp = context.User.GetClaimValueInternal(ClaimTypes.SerialNumber);

            if (!lockedOut && securityStamp == user.SecurityStamp)
            {
                context.Succeed(requirement);
            }
        }
    }
}