using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using MinimalHelpers.FluentValidation;
using OperationResults;
using OperationResults.AspNetCore.Http;

namespace BeachApplication.Endpoints;

public class ReservationsEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var reservationApiGroup = endpoints.MapGroup("/api/reservations");

        reservationApiGroup.MapDelete("{id:guid}", DeleteAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("DeleteReservation")
            .WithOpenApi();

        reservationApiGroup.MapGet("{id:guid}", GetAsync)
            .RequireAuthorization()
            .Produces<Reservation>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetReservation")
            .WithOpenApi();

        reservationApiGroup.MapGet(string.Empty, GetListAsync)
            .RequireAuthorization()
            .Produces<PaginatedList<Reservation>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithName("GetResevations")
            .WithOpenApi();

        reservationApiGroup.MapPost(string.Empty, InsertAsync)
            .RequireAuthorization()
            .WithValidation<SaveReservationRequest>()
            .Produces<Umbrella>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithName("InsertReservation")
            .WithOpenApi();

        reservationApiGroup.MapPut("{id:guid}", UpdateAsync)
            .RequireAuthorization()
            .WithValidation<SaveReservationRequest>()
            .Produces<Umbrella>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("UpdateReservation")
            .WithOpenApi();
    }

    private static async Task<IResult> DeleteAsync(IReservationService reservationService, Guid id, HttpContext httpContext)
    {
        var result = await reservationService.DeleteAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetAsync(IReservationService reservationService, Guid id, HttpContext httpContext)
    {
        var result = await reservationService.GetAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetListAsync(IReservationService reservationService, HttpContext httpContext, DateOnly? reservationDate = null, int pageIndex = 0, int itemsPerPage = 10, string orderBy = "StartOn")
    {
        var result = await reservationService.GetListAsync(reservationDate, pageIndex, itemsPerPage, orderBy);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> InsertAsync(IReservationService reservationService, SaveReservationRequest request, HttpContext httpContext)
    {
        var result = await reservationService.InsertAsync(request);
        return httpContext.CreateResponse(result, "GetReservation", new { id = result.Content?.Id });
    }

    private static async Task<IResult> UpdateAsync(IReservationService reservationService, Guid id, SaveReservationRequest request, HttpContext httpContext)
    {
        var result = await reservationService.UpdateAsync(id, request);
        return httpContext.CreateResponse(result);
    }
}