using System.Security.Claims;
using BeachApplication.DataAccessLayer.Entities.Identity;
using BeachApplication.Shared.Models;
using Microsoft.AspNetCore.Identity;
using MinimalHelpers.Routing;

namespace BeachApplication.Endpoints;

public class MeEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var meApiGroup = endpoints.MapGroup("/api/me").RequireAuthorization();

        meApiGroup.MapPost("/enable2fa", EnableTwoFactorAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithName("enable2fa")
            .WithOpenApi();

        meApiGroup.MapGet("/profile", GetProfileAsync)
            .Produces<User>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithName("profile")
            .WithOpenApi();
    }

    private static async Task<IResult> EnableTwoFactorAsync(UserManager<ApplicationUser> userManager, ClaimsPrincipal claimsPrincipal)
    {
        var user = await userManager.GetUserAsync(claimsPrincipal);
        user.TwoFactorEnabled = true;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return TypedResults.BadRequest(result.Errors.Select(e => e.Description));
        }

        return TypedResults.NoContent();
    }

    private static async Task<IResult> GetProfileAsync(UserManager<ApplicationUser> userManager, ClaimsPrincipal claimsPrincipal)
    {
        var dbUser = await userManager.GetUserAsync(claimsPrincipal);
        var user = new User
        {
            Id = dbUser.Id,
            FirstName = dbUser.FirstName,
            LastName = dbUser.LastName,
            Email = dbUser.Email
        };

        return TypedResults.Ok(user);
    }
}