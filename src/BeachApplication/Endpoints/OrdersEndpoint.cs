﻿using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Extensions;
using BeachApplication.Shared.Collections;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;

namespace BeachApplication.Endpoints;

public class OrdersEndpoint : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var orderApiGroup = endpoints.MapGroup("/api/orders");

        orderApiGroup.MapDelete("{id:guid}", DeleteAsync)
            .RequireAuthorization("UserActive")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        orderApiGroup.MapGet("{id:guid}", GetAsync)
            .RequireAuthorization("UserActive")
            .Produces<Order>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        orderApiGroup.MapGet(string.Empty, GetListAsync)
            .RequireAuthorization("UserActive")
            .Produces<ListResult<Order>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithOpenApi();

        orderApiGroup.MapPost(string.Empty, SaveAsync)
            .RequireAuthorization("UserActive")
            .WithValidator<SaveOrderRequest>()
            .Produces<Order>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesValidationProblem()
            .WithOpenApi();
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

    private static async Task<IResult> SaveAsync(IOrderService orderService, SaveOrderRequest request, HttpContext httpContext)
    {
        var result = await orderService.SaveAsync(request);
        return httpContext.CreateResponse(result);
    }
}