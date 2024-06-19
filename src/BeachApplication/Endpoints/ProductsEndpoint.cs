using BeachApplication.BusinessLayer.Services;
using BeachApplication.Extensions;
using BeachApplication.Shared.Collections;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;

namespace BeachApplication.Endpoints;

public class ProductsEndpoint : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var productsApiGroup = endpoints.MapGroup("/api/products");

        productsApiGroup.MapDelete("{id:guid}", DeleteAsync)
            .RequireAuthorization("Administrator")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        productsApiGroup.MapGet("{id:guid}", GetAsync)
            .RequireAuthorization("UserActive")
            .WithName("GetProduct")
            .Produces<Product>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        productsApiGroup.MapGet(string.Empty, GetListAsync)
            .RequireAuthorization("UserActive")
            .Produces<ListResult<Product>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithOpenApi();

        productsApiGroup.MapPost(string.Empty, InsertAsync)
            .RequireAuthorization("Administrator")
            .WithValidator<SaveProductRequest>()
            .Produces<Product>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status409Conflict)
            .ProducesValidationProblem()
            .WithOpenApi();

        productsApiGroup.MapPut("{id:guid}", UpdateAsync)
            .RequireAuthorization("Administrator")
            .WithValidator<SaveProductRequest>()
            .Produces<Product>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .WithOpenApi();
    }

    private static async Task<IResult> DeleteAsync(IProductService productService, Guid id, HttpContext httpContext)
    {
        var result = await productService.DeleteAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetAsync(IProductService productService, Guid id, HttpContext httpContext)
    {
        var result = await productService.GetAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetListAsync(IProductService productService, HttpContext httpContext, string name = null, string category = null, int pageIndex = 0, int itemsPerPage = 50, string orderBy = "Name, Price")
    {
        var result = await productService.GetListAsync(name, category, pageIndex, itemsPerPage, orderBy);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> InsertAsync(IProductService productService, SaveProductRequest request, HttpContext httpContext)
    {
        var result = await productService.InsertAsync(request);
        return httpContext.CreateResponse(result, "GetProduct", new { id = result.Content?.Id });
    }

    private static async Task<IResult> UpdateAsync(IProductService productService, Guid id, SaveProductRequest request, HttpContext httpContext)
    {
        var result = await productService.UpdateAsync(id, request);
        return httpContext.CreateResponse(result);
    }
}