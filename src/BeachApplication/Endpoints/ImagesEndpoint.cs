using System.Net.Mime;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Models;
using BeachApplication.Shared.Models;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;

namespace BeachApplication.Endpoints;

public class ImagesEndpoint : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var imagesApiGroup = endpoints.MapGroup("/api/images");

        imagesApiGroup.MapDelete("{id:guid}", DeleteAsync)
            .RequireAuthorization("Administrator")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        imagesApiGroup.MapGet("{id:guid}", GetAsync)
            .WithName("GetImage")
            .RequireAuthorization("UserActive")
            .Produces<Image>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        imagesApiGroup.MapGet(string.Empty, GetListAsync)
            .RequireAuthorization("UserActive")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithOpenApi();

        imagesApiGroup.MapGet("{id:guid}/image", ReadAsync)
            .RequireAuthorization("UserActive")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        imagesApiGroup.MapPost(string.Empty, UploadAsync)
            .Accepts<FormFileContent>(MediaTypeNames.Multipart.FormData)
            .RequireAuthorization("Administrator")
            .Produces<Image>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithOpenApi();
    }

    private static async Task<IResult> DeleteAsync(IImageService imageService, Guid id, HttpContext httpContext)
    {
        var result = await imageService.DeleteAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetAsync(IImageService imageService, Guid id, HttpContext httpContext)
    {
        var result = await imageService.GetAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetListAsync(IImageService imageService, HttpContext httpContext)
    {
        var result = await imageService.GetListAsync();
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> ReadAsync(IImageService imageService, Guid id, HttpContext httpContext)
    {
        var result = await imageService.ReadAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> UploadAsync(IImageService imageService, FormFileContent content, HttpContext httpContext)
    {
        var result = await imageService.UploadAsync(content.File.FileName, content.File.OpenReadStream(), content.Description, content.Overwrite);
        return httpContext.CreateResponse(result, "GetImage", new { id = result.Content?.Id });
    }
}