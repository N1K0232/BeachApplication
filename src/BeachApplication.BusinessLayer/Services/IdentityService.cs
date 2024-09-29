using System.Data;
using System.Net.Mime;
using System.Security.Claims;
using BeachApplication.BusinessLayer.Core;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.BusinessLayer.Settings;
using BeachApplication.DataAccessLayer;
using BeachApplication.DataAccessLayer.DataProtection;
using BeachApplication.DataAccessLayer.Entities.Identity;
using BeachApplication.DataAccessLayer.Extensions;
using BeachApplication.Shared.Models.Requests;
using BeachApplication.Shared.Models.Responses;
using FluentEmail.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OperationResults;
using SimpleAuthentication.JwtBearer;

namespace BeachApplication.BusinessLayer.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly IFluentEmail fluentEmail;
    private readonly LinkGenerator linkGenerator;
    private readonly ITimeLimitedDataProtectionService dataProtectionService;
    private readonly IJwtBearerService jwtBearerService;
    private readonly IQrCodeGeneratorService qrCodeGeneratorService;
    private readonly AppSettings appSettings;

    public IdentityService(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IFluentEmail fluentEmail,
        LinkGenerator linkGenerator,
        ITimeLimitedDataProtectionService dataProtectionService,
        IJwtBearerService jwtBearerService,
        IQrCodeGeneratorService qrCodeGeneratorService,
        IOptions<AppSettings> appSettingsOptions)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.fluentEmail = fluentEmail;
        this.linkGenerator = linkGenerator;
        this.dataProtectionService = dataProtectionService;
        this.jwtBearerService = jwtBearerService;
        this.qrCodeGeneratorService = qrCodeGeneratorService;
        appSettings = appSettingsOptions.Value;
    }

    public async Task<Result<ByteArrayFileContent>> GetQrCodeAsync(string token)
    {
        var userId = await dataProtectionService.UnprotectAsync(token);
        var user = await userManager.FindByIdAsync(userId);

        var authenticatorKeyExists = await userManager.AuthenticatorKeyExistsAsync(user);
        if (authenticatorKeyExists)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid request");
        }

        var qrCodeUri = await GenerateQrCodeUriAsync(user);
        var qrCodeBytes = await qrCodeGeneratorService.GenerateAsync(qrCodeUri);

        return new ByteArrayFileContent(qrCodeBytes, MediaTypeNames.Image.Png);
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await userManager.FindByNameAsync(request.UserName);
        if (user is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, "No user found");
        }

        var isEmailConfirmed = await userManager.IsEmailConfirmedAsync(user);
        if (!isEmailConfirmed)
        {
            return Result.Fail(FailureReasons.ClientError, "Your account isn't verified", "Please verify your email");
        }

        var signInResult = await signInManager.PasswordSignInAsync(user, request.Password, request.IsPersistent, true);
        if (!signInResult.Succeeded)
        {
            if (signInResult.RequiresTwoFactor)
            {
                var token = await dataProtectionService.ProtectAsync(user.Id.ToString(), TimeSpan.FromMinutes(5));
                return new AuthResponse(token);
            }

            if (signInResult.IsLockedOut)
            {
                return Result.Fail(FailureReasons.ClientError, "you're locked out", $"you're locked out until {user.LockoutEnd}");
            }

            await userManager.AccessFailedAsync(user);
            return Result.Fail(FailureReasons.ClientError, "Login failed", "Invalid username or password");
        }

        var accessToken = await CreateTokenAsync(user);
        return new AuthResponse(accessToken);
    }

    public async Task<Result> RegisterAsync(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email,
            PhoneNumber = request.PhoneNumber,
            TwoFactorEnabled = request.TwoFactorEnabled
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (result.Succeeded)
        {
            await SendVerificationEmailAsync(user);
            return Result.Ok();
        }

        return Result.Fail(FailureReasons.ClientError, "Couldn't registrate", result.GetErrors());
    }

    public async Task<Result<ResetPasswordResponse>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is not null)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            return new ResetPasswordResponse(token);
        }

        return Result.Fail(FailureReasons.ItemNotFound, "User not found", "User not found");
    }

    public async Task<Result> UpdatePasswordAsync(ChangePasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is not null)
        {
            var result = await userManager.ResetPasswordAsync(user, request.Token, request.Password);
            return result.Succeeded ? Result.Ok() : Result.Fail(FailureReasons.ClientError, "Password wasn't update", result.GetErrors());
        }

        return Result.Fail(FailureReasons.ItemNotFound, "User not found", "User not found");
    }

    public async Task<Result<AuthResponse>> ValidateTwoFactorAsync(TwoFactorRequest request)
    {
        var userId = await dataProtectionService.UnprotectAsync(request.Token);
        var user = await userManager.FindByIdAsync(userId);

        var authenticatorTokenProvider = userManager.Options.Tokens.AuthenticatorTokenProvider;
        var isValidTotpCode = await userManager.VerifyTwoFactorTokenAsync(user, authenticatorTokenProvider, request.Code);

        if (!isValidTotpCode)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid code");
        }

        var accessToken = await CreateTokenAsync(user);
        return new AuthResponse(accessToken);
    }

    public async Task<Result> VerifyEmailAsync(Guid userId, string token)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is not null)
        {
            var result = await userManager.ConfirmEmailAsync(user, token);
            var isEmailConfirmed = await userManager.IsEmailConfirmedAsync(user);

            if (result.Succeeded && isEmailConfirmed)
            {
                await userManager.AddToRoleAsync(user, RoleNames.User);
                return Result.Ok();
            }

            return Result.Fail(FailureReasons.ClientError, "Couldn't verify the email", "Email wasn't verified. Try again");
        }

        return Result.Fail(FailureReasons.ItemNotFound, "User not found", "User not found");
    }

    private async Task<string> CreateTokenAsync(ApplicationUser user)
    {
        var userRoles = await userManager.GetRolesAsync(user);
        await userManager.UpdateSecurityStampAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.SerialNumber, user.SecurityStamp ?? string.Empty)
        }.Union(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        return await jwtBearerService.CreateTokenAsync(user.UserName, claims.ToList());
    }

    private async Task<string> GenerateQrCodeUriAsync(ApplicationUser user)
    {
        await userManager.ResetAuthenticatorKeyAsync(user);
        var secret = await userManager.GetAuthenticatorKeyAsync(user);

        return $"otpauth://totp/{Uri.EscapeDataString(appSettings.ApplicationName)}:{user.Email}?secret={secret}&issuer={Uri.EscapeDataString(appSettings.ApplicationName)}";
    }

    private async Task SendVerificationEmailAsync(ApplicationUser user)
    {
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var httpContext = signInManager.Context;

        var scheme = httpContext.Request.Scheme;
        var values = new RouteValueDictionary
        {
            ["userId"] = user.Id,
            ["token"] = token
        };

        var confirmationLink = linkGenerator.GetUriByRouteValues(httpContext, "verifyemail", values, scheme);
        await fluentEmail.To(user.Email).Subject("Confirm your email")
            .Body($"Please confirm your email by clicking this link: <a href='{confirmationLink}'>Confirm Email</a>", true)
            .SendAsync();
    }
}