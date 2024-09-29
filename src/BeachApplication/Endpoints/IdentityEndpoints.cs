using System.Security.Claims;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.DataAccessLayer.Extensions;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using BeachApplication.Shared.Models.Responses;
using Microsoft.AspNetCore.Http.HttpResults;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;

namespace BeachApplication.Endpoints;

public class IdentityEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var identityApiGroup = endpoints.MapGroup("/api/auth");

        identityApiGroup.MapGet("/me", GetMe)
            .Produces<User>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization("UserActive")
            .WithName("getme")
            .WithOpenApi();

        identityApiGroup.MapGet("/qrcode", GetQrCodeAsync)
            .Produces<FileContentHttpResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous()
            .WithName("qrcode")
            .WithOpenApi();

        identityApiGroup.MapPost("/login", LoginAsync)
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous()
            .WithName("login")
            .WithOpenApi();

        identityApiGroup.MapPost("/register", RegisterAsync)
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("register")
            .WithOpenApi();

        identityApiGroup.MapPost("/resetpassword", ResetPasswordAsync)
            .Produces<ResetPasswordResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .AllowAnonymous()
            .WithName("resetpassword")
            .WithOpenApi();

        identityApiGroup.MapPost("/updatepassword", UpdatePasswordAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .AllowAnonymous()
            .WithName("updatepassword")
            .WithOpenApi();

        identityApiGroup.MapPost("/validatetwofactor", ValidateTwoFactorAsync)
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous()
            .WithName("validatetwofactor")
            .WithOpenApi();

        identityApiGroup.MapGet("/verifyemail", VerifyEmailAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .AllowAnonymous()
            .WithName("verifyemail")
            .WithOpenApi();
    }

    private static Results<Ok<User>, UnauthorizedHttpResult, ForbidHttpResult> GetMe(ClaimsPrincipal principal)
    {
        var user = new User
        {
            Id = principal.GetId(),
            FirstName = principal.GetClaimValue(ClaimTypes.GivenName),
            LastName = principal.GetClaimValue(ClaimTypes.Surname),
            Email = principal.GetEmail()
        };

        return TypedResults.Ok(user);
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

    private static async Task<IResult> UpdatePasswordAsync(IIdentityService identityService, ChangePasswordRequest request, HttpContext httpContext)
    {
        var result = await identityService.UpdatePasswordAsync(request);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> ValidateTwoFactorAsync(IIdentityService identityService, TwoFactorRequest request, HttpContext httpContext)
    {
        var result = await identityService.ValidateTwoFactorAsync(request);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> VerifyEmailAsync(IIdentityService identityService, Guid userId, string token, HttpContext httpContext)
    {
        var result = await identityService.VerifyEmailAsync(userId, token);
        return httpContext.CreateResponse(result);
    }
}