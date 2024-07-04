using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Shared.Models;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;

namespace BeachApplication.Endpoints;

public class MeEndpoint : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var meApiGroup = endpoints.MapGroup("/api/me");

        meApiGroup.MapGet(string.Empty, GetAsync)
            .RequireAuthorization("UserActive")
            .Produces<User>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithOpenApi();
    }

    private static async Task<IResult> GetAsync(IMeService meService, HttpContext httpContext)
    {
        var result = await meService.GetAsync();
        return httpContext.CreateResponse(result);
    }
}