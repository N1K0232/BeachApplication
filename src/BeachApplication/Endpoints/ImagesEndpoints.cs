﻿using System.Net.Mime;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Shared.Models;
using MinimalHelpers.Routing;
using OperationResults;
using OperationResults.AspNetCore.Http;

namespace BeachApplication.Endpoints;

public class ImagesEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var imagesApiGroup = endpoints.MapGroup("/api/images");

        imagesApiGroup.MapDelete("{id:guid}", DeleteAsync)
            .RequireAuthorization("admin", "poweruser")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("DeleteImage")
            .WithOpenApi();

        imagesApiGroup.MapGet("{id:guid}", GetAsync)
            .RequireAuthorization("admin", "poweruser", "user")
            .Produces<Image>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetImage")
            .WithOpenApi();

        imagesApiGroup.MapGet(string.Empty, GetListAsync)
            .RequireAuthorization("admin", "poweruser", "user")
            .Produces<PaginatedList<Image>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithName("GetImages")
            .WithOpenApi();

        imagesApiGroup.MapGet("{id:guid}/image", ReadAsync)
            .RequireAuthorization("admin", "poweruser", "user")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetImageContent")
            .WithOpenApi();

        imagesApiGroup.MapPost(string.Empty, UploadAsync)
            .Accepts<IFormFile>(MediaTypeNames.Multipart.FormData)
            .RequireAuthorization("admin", "poweruser")
            .Produces<Image>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .DisableAntiforgery()
            .WithName("UploadImage")
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

    private static async Task<IResult> UploadAsync(IImageService imageService, IFormFile file, HttpContext httpContext)
    {
        var result = await imageService.UploadAsync(file.FileName, file.OpenReadStream());
        return httpContext.CreateResponse(result, "GetImage", new { id = result.Content?.Id });
    }
}