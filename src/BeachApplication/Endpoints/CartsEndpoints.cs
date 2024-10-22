using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using MinimalHelpers.FluentValidation;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;

namespace BeachApplication.Endpoints;

public class CartsEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var cartApiGroup = endpoints.MapGroup("/api/carts");

        cartApiGroup.MapPost("confirm/{id:guid}", ConfirmAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("ConfirmCart")
            .WithOpenApi();

        cartApiGroup.MapDelete("{id:guid}", DeleteAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("DeleteCart")
            .WithOpenApi();

        cartApiGroup.MapGet(string.Empty, GetAsync)
            .RequireAuthorization()
            .Produces<Cart>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetCart")
            .WithOpenApi();

        cartApiGroup.MapDelete("{id:guid}/items/{itemId:guid}", RemoveItemAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("RemoveItem")
            .WithOpenApi();

        cartApiGroup.MapPost("save", SaveAsync)
            .RequireAuthorization()
            .WithValidation<SaveCartRequest>()
            .Produces<Cart>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithName("SaveCart")
            .WithOpenApi();
    }

    private static async Task<IResult> ConfirmAsync(ICartService cartService, Guid id, HttpContext httpContext)
    {
        var result = await cartService.ConfirmAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> DeleteAsync(ICartService cartService, Guid id, HttpContext httpContext)
    {
        var result = await cartService.DeleteAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetAsync(ICartService cartService, HttpContext httpContext)
    {
        var result = await cartService.GetAsync();
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> RemoveItemAsync(ICartService cartService, Guid id, Guid itemId, HttpContext httpContext)
    {
        var result = await cartService.RemoveItemAsync(id, itemId);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> SaveAsync(ICartService cartService, SaveCartRequest request, HttpContext httpContext)
    {
        var result = await cartService.SaveAsync(request);
        return httpContext.CreateResponse(result);
    }
}