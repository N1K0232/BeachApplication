using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Extensions;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;

namespace BeachApplication.Endpoints;

public class CategoriesEndpoint : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var categoriesApiGroup = endpoints.MapGroup("/api/categories");

        categoriesApiGroup.MapDelete("{id:guid}", DeleteAsync)
            .RequireAuthorization("Administrator")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        categoriesApiGroup.MapGet("{id:guid}", GetAsync)
            .RequireAuthorization("UserActive")
            .WithName("GetCategory")
            .Produces<Category>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        categoriesApiGroup.MapGet(string.Empty, GetListAsync)
            .RequireAuthorization("UserActive")
            .Produces<IEnumerable<Category>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithOpenApi();

        categoriesApiGroup.MapPost(string.Empty, InsertAsync)
            .RequireAuthorization("Administrator")
            .WithValidator<SaveCategoryRequest>()
            .Produces<Category>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status409Conflict)
            .ProducesValidationProblem()
            .WithOpenApi();

        categoriesApiGroup.MapPut("{id:guid}", UpdateAsync)
            .RequireAuthorization("Administrator")
            .WithValidator<SaveCategoryRequest>()
            .Produces<Category>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .WithOpenApi();
    }

    private static async Task<IResult> DeleteAsync(ICategoryService categoryService, Guid id, HttpContext httpContext)
    {
        var result = await categoryService.DeleteAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetAsync(ICategoryService categoryService, Guid id, HttpContext httpContext)
    {
        var result = await categoryService.GetAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetListAsync(ICategoryService categoryService, HttpContext httpContext, string name = null)
    {
        var result = await categoryService.GetListAsync(name);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> InsertAsync(ICategoryService categoryService, SaveCategoryRequest request, HttpContext httpContext)
    {
        var result = await categoryService.InsertAsync(request);
        return httpContext.CreateResponse(result, "GetCategory", new { id = result.Content?.Id });
    }

    private static async Task<IResult> UpdateAsync(ICategoryService categoryService, Guid id, SaveCategoryRequest request, HttpContext httpContext)
    {
        var result = await categoryService.UpdateAsync(id, request);
        return httpContext.CreateResponse(result);
    }
}