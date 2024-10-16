using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;

namespace BeachApplication.Endpoints;

public class PostsEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var postApiGroup = endpoints.MapGroup("/api/posts");

        postApiGroup.MapDelete("{id:guid}", DeleteAsync)
            .RequireAuthorization("admin", "poweruser")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("DeletePost")
            .WithOpenApi();

        postApiGroup.MapGet("{id:guid}", GetAsync)
            .RequireAuthorization("admin", "poweruser", "user")
            .Produces<Post>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetPost")
            .WithOpenApi();

        postApiGroup.MapGet(string.Empty, GetListAsync)
            .RequireAuthorization("admin", "poweruser", "user")
            .Produces<IEnumerable<Post>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithName("GetPostList")
            .WithOpenApi();

        postApiGroup.MapPost(string.Empty, InsertAsync)
            .RequireAuthorization("admin", "poweruser")
            .Produces<Post>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithName("InsertPost")
            .WithOpenApi();

        postApiGroup.MapPut("{id:guid}", UpdateAsync)
            .RequireAuthorization("admin", "poweruser")
            .Produces<Post>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("UpdatePost")
            .WithOpenApi();
    }

    private static async Task<IResult> DeleteAsync(IPostService postService, Guid id, HttpContext httpContext)
    {
        var result = await postService.DeleteAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetAsync(IPostService postService, Guid id, HttpContext httpContext)
    {
        var result = await postService.GetAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetListAsync(IPostService postService, HttpContext httpContext)
    {
        var result = await postService.GetListAsync();
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> InsertAsync(IPostService postService, SavePostRequest request, HttpContext httpContext)
    {
        var result = await postService.InsertAsync(request);
        return httpContext.CreateResponse(result, "GetPost", new { id = result.Content?.Id });
    }

    private static async Task<IResult> UpdateAsync(IPostService postService, Guid id, SavePostRequest request, HttpContext httpContext)
    {
        var result = await postService.UpdateAsync(id, request);
        return httpContext.CreateResponse(result);
    }
}