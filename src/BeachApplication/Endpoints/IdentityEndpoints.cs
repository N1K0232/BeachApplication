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
        var identityApiGroup = endpoints.MapGroup("/api/auth").AllowAnonymous();

        identityApiGroup.MapPost("/forgotpassword", ForgotPasswordAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("forgotpassword")
            .WithOpenApi();

        identityApiGroup.MapPost("/login", LoginAsync)
            .WithValidation<LoginRequest>()
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("login")
            .WithOpenApi();

        identityApiGroup.MapGet("/qrcode", GetQrCodeAsync)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("qrcode")
            .WithOpenApi();

        identityApiGroup.MapPost("/register", RegisterAsync)
            .WithValidation<RegisterRequest>()
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("register")
            .WithOpenApi();

        identityApiGroup.MapPost("/resetpassword", ResetPasswordAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("resetpassword")
            .WithOpenApi();

        identityApiGroup.MapPost("/validate2fa", ValidateAsync)
            .WithValidation<TwoFactorValidationRequest>()
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("validate2fa")
            .WithOpenApi();

        identityApiGroup.MapPost("/verifyemail", VerifyEmailAsync)
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

    private static async Task<IResult> LoginAsync(IIdentityService identityService, LoginRequest request, HttpContext httpContext)
    {
        var result = await identityService.LoginAsync(request);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetQrCodeAsync(IIdentityService identityService, string token, HttpContext httpContext)
    {
        var result = await identityService.GetQrCodeAsync(token);
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