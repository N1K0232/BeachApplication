using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using MinimalHelpers.FluentValidation;
using MinimalHelpers.Routing;
using OperationResults;
using OperationResults.AspNetCore.Http;

namespace BeachApplication.Endpoints;

public class OrdersEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var orderApiGroup = endpoints.MapGroup("/api/orders");

        orderApiGroup.MapPost("save", SaveAsync)
            .RequireAuthorization()
            .WithValidation<SaveOrderRequest>()
            .Produces<Order>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithName("SaveOrder")
            .WithOpenApi();

        orderApiGroup.MapDelete("{id:guid}", DeleteAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("DeleteOrder")
            .WithOpenApi();

        orderApiGroup.MapGet("{id:guid}", GetAsync)
            .WithName("GetOrder")
            .RequireAuthorization()
            .Produces<Order>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetOrder")
            .WithOpenApi();

        orderApiGroup.MapGet(string.Empty, GetListAsync)
            .RequireAuthorization()
            .Produces<PaginatedList<Order>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithName("GetOrders")
            .WithOpenApi();
    }

    private static async Task<IResult> SaveAsync(IOrderService orderService, SaveOrderRequest request, HttpContext httpContext)
    {
        var result = await orderService.SaveAsync(request);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> DeleteAsync(IOrderService orderService, Guid id, HttpContext httpContext)
    {
        var result = await orderService.DeleteAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetAsync(IOrderService orderService, Guid id, HttpContext httpContext)
    {
        var result = await orderService.GetAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetListAsync(IOrderService orderService, HttpContext httpContext, int pageIndex = 0, int itemsPerPage = 10, string orderBy = "OrderDate DESC, OrderTime DESC")
    {
        var result = await orderService.GetListAsync(pageIndex, itemsPerPage, orderBy);
        return httpContext.CreateResponse(result);
    }
}