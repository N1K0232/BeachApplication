using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Shared.Models.Requests;
using BeachApplication.Shared.Models.Responses;
using MinimalHelpers.FluentValidation;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;

namespace BeachApplication.Endpoints;

public class IdentityEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var identityApiGroup = endpoints.MapGroup("/api/auth");

        identityApiGroup.MapPost("/forgotpassword", ForgotPasswordAsync)
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("forgotpassword")
            .WithOpenApi();

        identityApiGroup.MapGet("/qrcode", GetQrCodeAsync)
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("qrcode")
            .WithOpenApi();

        identityApiGroup.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .WithValidation<LoginRequest>()
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("login")
            .WithOpenApi();

        identityApiGroup.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("logout")
            .WithOpenApi();

        identityApiGroup.MapPost("/refresh", RefreshTokenAsync)
            .AllowAnonymous()
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("refresh")
            .WithOpenApi();

        identityApiGroup.MapPost("/register", RegisterAsync)
            .AllowAnonymous()
            .WithValidation<RegisterRequest>()
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("register")
            .WithOpenApi();

        identityApiGroup.MapPost("/resetpassword", ResetPasswordAsync)
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("resetpassword")
            .WithOpenApi();

        identityApiGroup.MapPost("/validate2fa", ValidateAsync)
            .AllowAnonymous()
            .WithValidation<TwoFactorValidationRequest>()
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("validate2fa")
            .WithOpenApi();

        identityApiGroup.MapPost("/verifyemail", VerifyEmailAsync)
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("verifyemail")
            .WithOpenApi();
    }

    private static async Task<IResult> ForgotPasswordAsync(IIdentityService identityService, ForgotPasswordRequest request, HttpContext httpContext)
    {
        var result = await identityService.ForgotPasswordAsync(request);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetQrCodeAsync(IIdentityService identityService, string token, HttpContext httpContext)
    {
        var result = await identityService.GetQrCodeAsync(token);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> LoginAsync(IIdentityService identityService, LoginRequest request, HttpContext httpContext)
    {
        var result = await identityService.LoginAsync(request);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> LogoutAsync(IIdentityService identityService, HttpContext httpContext)
    {
        var result = await identityService.LogoutAsync();
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
        return httpContext.CreateResponse(result, StatusCodes.Status201Created);
    }

    private static async Task<IResult> ResetPasswordAsync(IIdentityService identityService, ResetPasswordRequest request, HttpContext httpContext)
    {
        var result = await identityService.ResetPasswordAsync(request);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> ValidateAsync(IIdentityService identityService, TwoFactorValidationRequest request, HttpContext httpContext)
    {
        var result = await identityService.ValidateAsync(request);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> VerifyEmailAsync(IIdentityService identityService, VerifyEmailRequest request, HttpContext httpContext)
    {
        var result = await identityService.VerifyEmailAsync(request);
        return httpContext.CreateResponse(result);
    }
}