using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using MinimalHelpers.Routing;
using OperationResults;
using OperationResults.AspNetCore.Http;

namespace BeachApplication.Endpoints;

public class UmbrellasEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var umbrellasApiGroup = endpoints.MapGroup("/api/umbrellas");

        umbrellasApiGroup.MapDelete("{id:guid}", DeleteAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("DeleteUmbrella")
            .WithOpenApi();

        umbrellasApiGroup.MapGet("{id:guid}", GetAsync)
            .RequireAuthorization()
            .Produces<Umbrella>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetUmbrella")
            .WithOpenApi();

        umbrellasApiGroup.MapGet(string.Empty, GetListAsync)
            .RequireAuthorization()
            .Produces<PaginatedList<Umbrella>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithName("GetUmbrellas")
            .WithOpenApi();

        umbrellasApiGroup.MapPost(string.Empty, InsertAsync)
            .RequireAuthorization()
            .Produces<Umbrella>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithName("InsertUmbrella")
            .WithOpenApi();

        umbrellasApiGroup.MapPut("{id:guid}", UpdateAsync)
            .RequireAuthorization()
            .Produces<Umbrella>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("UpdateUmbrella")
            .WithOpenApi();
    }

    private static async Task<IResult> DeleteAsync(IUmbrellaService umbrellaService, Guid id, HttpContext httpContext)
    {
        var result = await umbrellaService.DeleteAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetAsync(IUmbrellaService umbrellaService, Guid id, HttpContext httpContext)
    {
        var result = await umbrellaService.GetAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetListAsync(IUmbrellaService umbrellaService, HttpContext httpContext, char? letter = null, int pageIndex = 0, int itemsPerPage = 10)
    {
        var result = await umbrellaService.GetListAsync(letter, pageIndex, itemsPerPage);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> InsertAsync(IUmbrellaService umbrellaService, SaveUmbrellaRequest request, HttpContext httpContext)
    {
        var result = await umbrellaService.InsertAsync(request);
        return httpContext.CreateResponse(result, "GetReservation", new { id = result.Content?.Id });
    }

    private static async Task<IResult> UpdateAsync(IUmbrellaService umbrellaService, Guid id, SaveUmbrellaRequest request, HttpContext httpContext)
    {
        var result = await umbrellaService.UpdateAsync(id, request);
        return httpContext.CreateResponse(result);
    }
}