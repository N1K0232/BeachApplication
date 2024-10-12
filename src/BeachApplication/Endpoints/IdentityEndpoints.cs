﻿using System.Net.Mime;
using System.Security.Claims;
using AutoMapper;
using BeachApplication.DataAccessLayer;
using BeachApplication.DataAccessLayer.Entities.Identity;
using BeachApplication.Shared.Models.Requests;
using BeachApplication.Shared.Models.Responses;
using FluentEmail.Core;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using MinimalHelpers.FluentValidation;
using MinimalHelpers.Routing;
using QRCoder;
using SimpleAuthentication.JwtBearer;
using TinyHelpers.Extensions;

namespace BeachApplication.Endpoints;

public class IdentityEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var identityApiGroup = endpoints.MapGroup("/api/auth");

        identityApiGroup.MapPost("/login", LoginAsync)
            .WithValidation<LoginRequest>()
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous()
            .WithName("login")
            .WithOpenApi();

        identityApiGroup.MapGet("/qrcode", GetQrCodeAsync)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous()
            .WithName("qrcode")
            .WithOpenApi();

        identityApiGroup.MapPost("/register", RegisterAsync)
            .WithValidation<RegisterRequest>()
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous()
            .WithName("register")
            .WithOpenApi();

        identityApiGroup.MapPost("/validate2fa", ValidateAsync)
            .WithValidation<TwoFactorValidationRequest>()
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous()
            .WithName("validate2fa")
            .WithOpenApi();

        identityApiGroup.MapGet("/verifyemail", VerifyEmailAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .AllowAnonymous()
            .WithName("verifyemail")
            .WithOpenApi();
    }

    private static async Task<IResult> LoginAsync(SignInManager<ApplicationUser> signInManager, ITimeLimitedDataProtector dataProtector, IJwtBearerService jwtBearerService, LoginRequest request)
    {
        var user = await signInManager.UserManager.FindByEmailAsync(request.Email);
        var result = await signInManager.PasswordSignInAsync(user, request.Password, request.IsPersistent, true);

        if (!result.Succeeded)
        {
            var isEmailConfirmed = await signInManager.UserManager.IsEmailConfirmedAsync(user);
            if (!isEmailConfirmed)
            {
                return TypedResults.BadRequest("You have to verify your account first");
            }

            var isLockedOut = await signInManager.UserManager.IsLockedOutAsync(user);
            if (isLockedOut)
            {
                return TypedResults.BadRequest("Your account is locked out");
            }

            if (result.RequiresTwoFactor)
            {
                var token = dataProtector.Protect(user.Id.ToString(), TimeSpan.FromMinutes(15));
                return TypedResults.Ok(new AuthResponse(token));
            }

            await signInManager.UserManager.AccessFailedAsync(user);
            return TypedResults.BadRequest("Invalid username or password");
        }

        var userRoles = await signInManager.UserManager.GetRolesAsync(user);
        await signInManager.UserManager.UpdateSecurityStampAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.SerialNumber, user.SecurityStamp)
        }
        .Union(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        var accessToken = await jwtBearerService.CreateTokenAsync(user.UserName, claims.ToList());
        return TypedResults.Ok(new AuthResponse(accessToken));
    }

    private static async Task<IResult> GetQrCodeAsync(UserManager<ApplicationUser> userManager, ITimeLimitedDataProtector dataProtector, IWebHostEnvironment environment, QRCodeGenerator qrCodeGenerator, string token)
    {
        ApplicationUser user = null;

        try
        {
            var userId = dataProtector.Unprotect(token);
            user = await userManager.FindByIdAsync(userId);
        }
        catch
        {
            return TypedResults.BadRequest();
        }

        if (user is null || (await userManager.GetAuthenticatorKeyAsync(user)).HasValue())
        {
            return TypedResults.BadRequest();
        }

        await userManager.ResetAuthenticatorKeyAsync(user);
        var secret = await userManager.GetAuthenticatorKeyAsync(user);

        var qrCodeUri = $"otpauth://totp/{Uri.EscapeDataString(environment.ApplicationName)}:{user.Email}?secret={secret}&issuer={Uri.EscapeDataString(environment.ApplicationName)}";

        using var qrCodeData = qrCodeGenerator.CreateQrCode(qrCodeUri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);

        var qrCodeBytes = qrCode.GetGraphic(3);
        return TypedResults.File(qrCodeBytes, MediaTypeNames.Image.Png);
    }

    private static async Task<IResult> RegisterAsync(UserManager<ApplicationUser> userManager, IMapper mapper, IFluentEmail fluentEmail, LinkGenerator linkGenerator, HttpContext httpContext, RegisterRequest request)
    {
        var user = mapper.Map<ApplicationUser>(request);
        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return TypedResults.BadRequest(result.Errors.Select(e => e.Description));
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var scheme = httpContext.Request.Scheme;

        var values = new RouteValueDictionary
        {
            ["userId"] = user.Id,
            ["token"] = token
        };

        var endpoint = linkGenerator.GetUriByRouteValues(httpContext, "verifyemail", values, scheme);
        var sendResult = await fluentEmail.To(user.Email).Subject("Confirm your email")
            .Body($"Please confirm your email by clicking this link: <a href='{endpoint}'>Confirm Email</a>", true)
            .SendAsync();

        if (!sendResult.Successful)
        {
            return TypedResults.BadRequest(sendResult.ErrorMessages);
        }

        return TypedResults.Created();
    }

    private static async Task<IResult> ValidateAsync(UserManager<ApplicationUser> userManager, ITimeLimitedDataProtector dataProtector, IJwtBearerService jwtBearerService, TwoFactorValidationRequest request)
    {
        ApplicationUser user = null;

        try
        {
            var userId = dataProtector.Unprotect(request.Token);
            user = await userManager.FindByIdAsync(userId);
        }
        catch
        {
            return TypedResults.BadRequest();
        }

        if (user is null)
        {
            return TypedResults.BadRequest();
        }

        var tokenProvider = userManager.Options.Tokens.AuthenticatorTokenProvider;
        var isValidTotpCode = await userManager.VerifyTwoFactorTokenAsync(user, tokenProvider, request.Code);

        if (!isValidTotpCode)
        {
            return TypedResults.BadRequest();
        }

        var userRoles = await userManager.GetRolesAsync(user);
        await userManager.UpdateSecurityStampAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.SerialNumber, user.SecurityStamp)
        }
        .Union(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        var accessToken = await jwtBearerService.CreateTokenAsync(user.UserName, claims.ToList());
        return TypedResults.Ok(new AuthResponse(accessToken));
    }

    private static async Task<IResult> VerifyEmailAsync(UserManager<ApplicationUser> userManager, string userId, string token)
    {
        var user = await userManager.FindByIdAsync(userId);
        var result = await userManager.ConfirmEmailAsync(user, token);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, RoleNames.User);
            return TypedResults.NoContent();
        }

        return TypedResults.BadRequest("Couldn't verify your email");
    }
}