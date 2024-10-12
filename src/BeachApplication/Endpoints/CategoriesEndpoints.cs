using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.DataAccessLayer;
using BeachApplication.DataAccessLayer.Authorization;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using MinimalHelpers.FluentValidation;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;

namespace BeachApplication.Endpoints;

public class CategoriesEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var categoriesApiGroup = endpoints.MapGroup("/api/categories");

        categoriesApiGroup.MapDelete("{id:guid}", DeleteAsync)
            .RequireAuthorization(policy =>
            {
                policy.RequireAuthenticatedUser().RequireRole(RoleNames.Administrator, RoleNames.PowerUser);
                policy.Requirements.Add(new UserActiveRequirement());
            })
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
            .RequireAuthorization(policy =>
            {
                policy.RequireAuthenticatedUser().RequireRole(RoleNames.Administrator, RoleNames.PowerUser);
                policy.Requirements.Add(new UserActiveRequirement());
            })
            .WithValidation<SaveCategoryRequest>()
            .Produces<Category>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status409Conflict)
            .ProducesValidationProblem()
            .WithOpenApi();

        categoriesApiGroup.MapPut("{id:guid}", UpdateAsync)
            .RequireAuthorization(policy =>
            {
                policy.RequireAuthenticatedUser().RequireRole(RoleNames.Administrator, RoleNames.PowerUser);
                policy.Requirements.Add(new UserActiveRequirement());
            })
            .WithValidation<SaveCategoryRequest>()
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