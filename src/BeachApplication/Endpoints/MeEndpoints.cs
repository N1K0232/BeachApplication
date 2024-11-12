using System.Security.Claims;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Shared.Models;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;

namespace BeachApplication.Endpoints;

public class MeEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var meApiGroup = endpoints.MapGroup("/api/me").RequireAuthorization("UserActive");

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

    private static async Task<IResult> EnableTwoFactorAsync(IMeService meService, ClaimsPrincipal principal, HttpContext httpContext)
    {
        var result = await meService.EnableTwoFactorAsync(principal);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetProfileAsync(IMeService meService, ClaimsPrincipal principal, HttpContext httpContext)
    {
        var result = await meService.GetAsync(principal);
        return httpContext.CreateResponse(result);
    }
}