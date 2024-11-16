using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Shared.Models;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;

namespace BeachApplication.Endpoints;

public class MeEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var meApiGroup = endpoints.MapGroup("/api/me").RequireAuthorization();

        meApiGroup.MapDelete("/deleteprofilephoto", DeleteProfilePhotoAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("deleteprofilephoto")
            .WithOpenApi();

        meApiGroup.MapPost("/enable2fa", EnableTwoFactorAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("enable2fa")
            .WithOpenApi();

        meApiGroup.MapGet("/profile", GetProfileAsync)
            .Produces<User>(StatusCodes.Status200OK)
            .WithName("profile")
            .WithOpenApi();

        meApiGroup.MapGet("/profilephoto", GetProfilePhotoAsync)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("profilephoto")
            .WithOpenApi();

        meApiGroup.MapPost("/updateprofilephoto", UpdateProfilePhotoAsync)
            .DisableAntiforgery()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("updateprofilephoto")
            .WithOpenApi();
    }

    private static async Task<IResult> DeleteProfilePhotoAsync(IMeService meService, HttpContext httpContext)
    {
        var result = await meService.DeleteProfilePhotoAsync();
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> EnableTwoFactorAsync(IMeService meService, HttpContext httpContext)
    {
        var result = await meService.EnableTwoFactorAsync();
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetProfileAsync(IMeService meService, HttpContext httpContext)
    {
        var result = await meService.GetAsync();
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetProfilePhotoAsync(IMeService meService, HttpContext httpContext)
    {
        var result = await meService.GetProfilePhotoAsync();
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> UpdateProfilePhotoAsync(IMeService meService, IFormFile file, HttpContext httpContext)
    {
        using var stream = file.OpenReadStream();
        var result = await meService.UpdateProfilePhotoAsync(stream, file.FileName);

        return httpContext.CreateResponse(result);
    }
}