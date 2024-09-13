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
        var authApiGroup = endpoints.MapGroup("/api/auth").AllowAnonymous();

        authApiGroup.MapPost("login", LoginAsync)
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("login")
            .WithOpenApi();

        authApiGroup.MapPost("register", RegisterAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("register")
            .WithOpenApi();

        authApiGroup.MapPost("resetpassword", ResetPasswordAsync)
            .Produces<ResetPasswordResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("resetpassword")
            .WithOpenApi();

        authApiGroup.MapPost("updatepassword", UpdatePasswordAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("updatepassword")
            .WithOpenApi();

        authApiGroup.MapGet("verifyemail", VerifyEmailAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("verifyemail")
            .WithOpenApi();
    }

    private static async Task<IResult> LoginAsync(IIdentityService identityService, LoginRequest request, HttpContext httpContext)
    {
        var result = await identityService.LoginAsync(request);
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

    private static async Task<IResult> VerifyEmailAsync(IIdentityService identityService, Guid userId, string token, HttpContext httpContext)
    {
        var result = await identityService.VerifyEmailAsync(userId, token);
        return httpContext.CreateResponse(result);
    }
}