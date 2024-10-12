using System.Security.Claims;
using BeachApplication.DataAccessLayer.Entities.Identity;
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
            var user = await userManager.GetUserAsync(context.User);
            var lockedOut = await userManager.IsLockedOutAsync(user);

            var securityStamp = context.User.FindFirstValue(ClaimTypes.SerialNumber);
            if (!lockedOut && securityStamp == user.SecurityStamp)
            {
                context.Succeed(requirement);
            }
        }
    }
}