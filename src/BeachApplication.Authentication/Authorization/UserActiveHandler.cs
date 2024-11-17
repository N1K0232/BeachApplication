using System.Security.Claims;
using BeachApplication.Authentication.Entities;
using BeachApplication.Authentication.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BeachApplication.Authorization;

public class UserActiveHandler(UserManager<ApplicationUser> userManager) : AuthorizationHandler<UserActiveRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UserActiveRequirement requirement)
    {
        var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
        if (isAuthenticated)
        {
            var user = await userManager.GetUserAsync(context.User);
            var lockedOut = await userManager.IsLockedOutAsync(user);

            var securityStamp = context.User.GetClaimValue(ClaimTypes.SerialNumber);
            var applicationId = context.User.GetClaimValue(ClaimTypes.PrimarySid);
            var clientId = context.User.GetClaimValue(ClaimTypes.PrimaryGroupSid);

            if (!lockedOut &&
                securityStamp == user.SecurityStamp &&
                applicationId == requirement.ApplicationId &&
                clientId == requirement.ClientId)
            {
                context.Succeed(requirement);
            }
        }
    }
}