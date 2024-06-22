using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Shared.Models.Requests;
using BeachApplication.Shared.Models.Responses;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;

namespace BeachApplication.Endpoints;

public class AuthEndpoint : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var authApiGroup = endpoints.MapGroup("/api/auth");

        authApiGroup.MapPost("login", LoginAsync)
            .AllowAnonymous()
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithOpenApi();

        authApiGroup.MapPost("refresh", RefreshTokenAsync)
            .AllowAnonymous()
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithOpenApi();

        authApiGroup.MapPost("register", RegisterAsync)
            .AllowAnonymous()
            .Produces<RegisterResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithOpenApi();

        authApiGroup.MapPost("resetpassword", ResetPasswordAsync)
            .AllowAnonymous()
            .Produces<ResetPasswordResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        authApiGroup.MapPost("updatepassword", UpdatePasswordAsync)
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        authApiGroup.MapPost("verifyemail", VerifyEmailAsync)
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();
    }

    private static async Task<IResult> LoginAsync(IIdentityService identityService, LoginRequest request, HttpContext httpContext)
    {
        var result = await identityService.LoginAsync(request);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> RefreshTokenAsync(IIdentityService identityService, RefreshTokenRequest request, HttpContext httpContext)
    {
        var result = await identityService.RefreshTokenAsync(request);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> RegisterAsync(IIdentityService identityService, RegisterRequest request, HttpContext httpContext)
    {
        var result = await identityService.RegisterAsync(request);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> ResetPasswordAsync(IIdentityService identityService, ResetPasswordRequest request, HttpContext httpContext)
    {
        var result = await identityService.ResetPasswordAsync(request);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> UpdatePasswordAsync(IIdentityService identityService, ChangePasswordRequest request, HttpContext httpContext)
    {
        var result = await identityService.UpdatePasswordAsync(request);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> VerifyEmailAsync(IIdentityService identityService, VerifyEmailRequest request, HttpContext httpContext)
    {
        var result = await identityService.VerifyEmailAsync(request);
        return httpContext.CreateResponse(result);
    }
}