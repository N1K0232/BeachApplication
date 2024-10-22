using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using MinimalHelpers.FluentValidation;
using MinimalHelpers.Routing;
using OperationResults;
using OperationResults.AspNetCore.Http;

namespace BeachApplication.Endpoints;

public class CommentsEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var commentsApiGroup = endpoints.MapGroup("/api/comments").RequireAuthorization();

        commentsApiGroup.MapDelete("{id:guid}", DeleteAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("DeleteComment")
            .WithOpenApi();

        commentsApiGroup.MapGet(string.Empty, GetUserCommentAsync)
            .Produces<Comment>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetUserComment")
            .WithOpenApi();

        commentsApiGroup.MapGet("{id:guid}", GetAsync)
            .Produces<Comment>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetComment")
            .WithOpenApi();

        commentsApiGroup.MapGet(string.Empty, GetListAsync)
            .Produces<PaginatedList<Comment>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithName("GetComments")
            .WithOpenApi();

        commentsApiGroup.MapPost(string.Empty, InsertAsync)
            .WithValidation<SaveCommentRequest>()
            .Produces<Comment>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("NewComment")
            .WithOpenApi();

        commentsApiGroup.MapPut("{id:guid}", UpdateAsync)
            .WithValidation<SaveCommentRequest>()
            .Produces<Comment>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("UpdateComment")
            .WithOpenApi();
    }

    private static async Task<IResult> DeleteAsync(ICommentService commentService, Guid id, HttpContext httpContext)
    {
        var result = await commentService.DeleteAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetUserCommentAsync(ICommentService commentService, HttpContext httpContext)
    {
        var result = await commentService.GetAsync();
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetAsync(ICommentService commentService, Guid id, HttpContext httpContext)
    {
        var result = await commentService.GetAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetListAsync(ICommentService commentService, HttpContext httpContext, int pageIndex = 0, int itemsPerPage = 10)
    {
        var result = await commentService.GetListAsync(pageIndex, itemsPerPage);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> InsertAsync(ICommentService commentService, SaveCommentRequest request, HttpContext httpContext)
    {
        var result = await commentService.InsertAsync(request);
        return httpContext.CreateResponse(result, "GetComment", new { id = result.Content?.Id });
    }

    private static async Task<IResult> UpdateAsync(ICommentService commentService, Guid id, SaveCommentRequest request, HttpContext httpContext)
    {
        var result = await commentService.UpdateAsync(id, request);
        return httpContext.CreateResponse(result);
    }
}