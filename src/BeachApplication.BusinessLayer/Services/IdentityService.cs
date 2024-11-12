using System.Net.Mime;
using System.Security.Claims;
using AutoMapper;
using BeachApplication.Authentication;
using BeachApplication.Authentication.DataProtection;
using BeachApplication.Authentication.Entities;
using BeachApplication.Authentication.JwtBearer;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Contracts;
using BeachApplication.Shared.Models.Requests;
using BeachApplication.Shared.Models.Responses;
using FluentEmail.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.IdentityModel.JsonWebTokens;
using OperationResults;
using TinyHelpers.Extensions;

namespace BeachApplication.BusinessLayer.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly LinkGenerator linkGenerator;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IQRCodeGeneratorService qrCodeGeneratorService;
    private readonly IDataProtectionService dataProtectionService;
    private readonly IJwtBearerService jwtBearerService;
    private readonly IFluentEmail fluentEmail;
    private readonly IMapper mapper;

    public IdentityService(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor,
        IQRCodeGeneratorService qrCodeGeneratorService,
        IDataProtectionService dataProtectionService,
        IJwtBearerService jwtBearerService,
        IFluentEmail fluentEmail,
        IMapper mapper)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.linkGenerator = linkGenerator;
        this.httpContextAccessor = httpContextAccessor;
        this.qrCodeGeneratorService = qrCodeGeneratorService;
        this.dataProtectionService = dataProtectionService;
        this.jwtBearerService = jwtBearerService;
        this.fluentEmail = fluentEmail;
        this.mapper = mapper;
    }

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Result.Fail(FailureReasons.ClientError);
        }

        var secret = await dataProtectionService.ProtectAsync(request.Email, TimeSpan.FromMinutes(15));
        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        var resetPasswordPage = linkGenerator.GetUriByPage
        (
            httpContextAccessor.HttpContext,
            "/Accounts/ResetPassword",
            null,
            new { secret, token }
        );

        var message = $$"""
            Someone just requested a password change for your account.
            If it was you, please follow the link below, otherwise report an abuse to our email address.
            ---

            {{resetPasswordPage}}

            ---
            Thank you!
            """;

        var sendResponse = await fluentEmail.To(request.Email)
            .Subject("Reset password")
            .Body(message, true)
            .SendAsync();

        if (!sendResponse.Successful)
        {
            var errors = string.Join(',', sendResponse.ErrorMessages);
            return Result.Fail(FailureReasons.ClientError, errors);
        }

        return Result.Ok();
    }

    public async Task<Result<ByteArrayFileContent>> GetQrCodeAsync(string token)
    {
        ApplicationUser user;

        try
        {
            var userId = await dataProtectionService.UnprotectAsync(token);
            user = await userManager.FindByIdAsync(userId);
        }
        catch
        {
            return Result.Fail(FailureReasons.ClientError);
        }

        if (user is null || await HasAuthenticatorKeyAsync(user))
        {
            return Result.Fail(FailureReasons.ClientError);
        }

        var secret = await ResetAndGetAuthenticatorKeyAsync(user);
        var qrCodeBytes = await qrCodeGeneratorService.GenerateAsync(user.Email, secret);

        return new ByteArrayFileContent(qrCodeBytes, MediaTypeNames.Image.Png);
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        var result = await signInManager.PasswordSignInAsync(user, request.Password, request.IsPersistent, true);

        if (!result.Succeeded)
        {
            var isEmailConfirmed = await userManager.IsEmailConfirmedAsync(user);
            if (!isEmailConfirmed)
            {
                return Result.Fail(FailureReasons.ClientError, "You have to confirm your account first");
            }

            var isLockedOut = await userManager.IsLockedOutAsync(user);
            if (isLockedOut)
            {
                return Result.Fail(FailureReasons.ClientError, $"Your account is locked until {user.LockoutEnd}");
            }

            if (result.RequiresTwoFactor)
            {
                var token = await dataProtectionService.ProtectAsync(user.Id.ToString(), TimeSpan.FromMinutes(15));
                return new AuthResponse(token);
            }

            await userManager.AccessFailedAsync(user);
            return Result.Fail(FailureReasons.ClientError, "Invalid email or password");
        }

        return await CreateTokenAsync(user);
    }

    public async Task<Result> RegisterAsync(RegisterRequest request)
    {
        var user = mapper.Map<ApplicationUser>(request);
        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var detail = string.Join(',', result.Errors.Select(e => e.Description));
            return Result.Fail(FailureReasons.ClientError, "Couldn't registrate", detail);
        }

        var secret = await dataProtectionService.ProtectAsync(user.Id.ToString(), TimeSpan.FromMinutes(15));
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        var verifyEmailPage = linkGenerator.GetUriByPage
        (
            httpContextAccessor.HttpContext,
            "/Accounts/VerifyEmail",
            null,
            new { secret, token }
        );

        var message = $$"""
            Good evening,

            It looks like you successfully subscribed to our website.
            However, before you could have access and log-in, we need you to verify your account first.

            In order to do this, you should click on the link below.
            It will redirect you to the verification page and will automatically verify your email address.
            ---

            {{verifyEmailPage}}

            ---
            Thank you for the attention.
            All the best.

            The developer team of the beach application
            """;

        var sendResponse = await fluentEmail.To(user.Email)
            .Subject("Verify your email")
            .Body(message, true)
            .SendAsync();

        if (!sendResponse.Successful)
        {
            var detail = string.Join(',', sendResponse.ErrorMessages);
            return Result.Fail(FailureReasons.ClientError, "Couldn't send the email", detail);
        }

        await userManager.AddToRoleAsync(user, RoleNames.User);
        return Result.Ok();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
    {
        ApplicationUser user;

        try
        {
            var email = await dataProtectionService.UnprotectAsync(request.Secret);
            user = await userManager.FindByEmailAsync(email);
        }
        catch
        {
            return Result.Fail(FailureReasons.ClientError);
        }

        if (user is null)
        {
            return Result.Fail(FailureReasons.ClientError);
        }

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            return Result.Fail(FailureReasons.ClientError);
        }

        var message = """
            Your password was successfully changed!
            Now you can navigate in the website again
            """;

        var sendResponse = await fluentEmail.To(user.Email)
            .Subject("Password changed")
            .Body(message)
            .SendAsync();

        if (!sendResponse.Successful)
        {
            var detail = string.Join(',', sendResponse.ErrorMessages);
            return Result.Fail(FailureReasons.ClientError, "Couldn't send the email", detail);
        }

        return Result.Ok();
    }

    public async Task<Result<AuthResponse>> ValidateAsync(TwoFactorValidationRequest request)
    {
        ApplicationUser user;

        try
        {
            var userId = await dataProtectionService.UnprotectAsync(request.Token);
            user = await userManager.FindByIdAsync(userId);
        }
        catch
        {
            return Result.Fail(FailureReasons.ClientError);
        }

        if (user is null)
        {
            return Result.Fail(FailureReasons.ClientError);
        }

        var tokenProvider = userManager.Options.Tokens.AuthenticatorTokenProvider;
        var isValidTotpCode = await userManager.VerifyTwoFactorTokenAsync(user, tokenProvider, request.Code);

        if (!isValidTotpCode)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid two-factor code");
        }

        return await CreateTokenAsync(user);
    }

    public async Task<Result> VerifyEmailAsync(VerifyEmailRequest request)
    {
        ApplicationUser user;

        try
        {
            var userId = await dataProtectionService.UnprotectAsync(request.Secret);
            user = await userManager.FindByIdAsync(userId);
        }
        catch
        {
            return Result.Fail(FailureReasons.ClientError);
        }

        if (user is null)
        {
            return Result.Fail(FailureReasons.ClientError);
        }

        var result = await userManager.ConfirmEmailAsync(user, request.Token);
        if (!result.Succeeded)
        {
            return Result.Fail(FailureReasons.ClientError, "Couldn't verify your email", result.Errors.FirstOrDefault().Description);
        }

        var message = """
            Your account was successfully verified
            Now you have access to our website!

            All the best :)
            """;

        var sendResponse = await fluentEmail.To(user.Email)
            .Subject("Email verified")
            .Body(message)
            .SendAsync();

        if (!sendResponse.Successful)
        {
            var errors = string.Join(',', sendResponse.ErrorMessages);
            return Result.Fail(FailureReasons.ClientError, errors);
        }

        return Result.Ok();
    }

    private async Task<AuthResponse> CreateTokenAsync(ApplicationUser user)
    {
        var userRoles = await userManager.GetRolesAsync(user);
        await userManager.UpdateSecurityStampAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.SerialNumber, user.SecurityStamp)
        }
        .Union(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        var accessToken = await jwtBearerService.CreateTokenAsync(user.UserName, claims.ToList());
        return new AuthResponse(accessToken);
    }

    private async Task<bool> HasAuthenticatorKeyAsync(ApplicationUser user)
    {
        var secret = await userManager.GetAuthenticatorKeyAsync(user);
        return secret.HasValue();
    }

    private async Task<string> ResetAndGetAuthenticatorKeyAsync(ApplicationUser user)
    {
        await userManager.ResetAuthenticatorKeyAsync(user);
        return await userManager.GetAuthenticatorKeyAsync(user);
    }
}