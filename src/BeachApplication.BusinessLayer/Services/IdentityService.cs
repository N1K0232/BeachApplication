using System.Net.Mime;
using System.Security.Claims;
using System.Security.Cryptography;
using AutoMapper;
using BeachApplication.Authentication;
using BeachApplication.Authentication.DataProtection;
using BeachApplication.Authentication.Entities;
using BeachApplication.Authentication.Extensions;
using BeachApplication.Authentication.JwtBearer;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.BusinessLayer.Settings;
using BeachApplication.Contracts;
using BeachApplication.Shared.Models.Requests;
using BeachApplication.Shared.Models.Responses;
using FluentEmail.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using OperationResults;
using TinyHelpers.Extensions;

namespace BeachApplication.BusinessLayer.Services;

public class IdentityService : IIdentityService
{
    private const string RefreshTokenKey = "RefreshToken";
    private const string RefreshTokenExpirationKey = "RefreshTokenExpirationDate";

    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly IUrlGeneratorService urlGeneratorService;
    private readonly IQRCodeGeneratorService qrCodeGeneratorService;
    private readonly IDataProtectionService dataProtectionService;
    private readonly IJwtBearerService jwtBearerService;
    private readonly IFluentEmail fluentEmail;
    private readonly IMapper mapper;

    private string applicationName;
    private TimeSpan refreshTokenExpirationTime;

    public IdentityService(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IUrlGeneratorService urlGeneratorService,
        IQRCodeGeneratorService qrCodeGeneratorService,
        IDataProtectionService dataProtectionService,
        IJwtBearerService jwtBearerService,
        IFluentEmail fluentEmail,
        IMapper mapper,
        IOptions<AppSettings> appSettingsOptions,
        IOptions<JwtBearerSettings> jwtBearerSettingsOptions)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.urlGeneratorService = urlGeneratorService;
        this.qrCodeGeneratorService = qrCodeGeneratorService;
        this.dataProtectionService = dataProtectionService;
        this.jwtBearerService = jwtBearerService;
        this.fluentEmail = fluentEmail;
        this.mapper = mapper;

        GetApplicationName(appSettingsOptions.Value);
        GetRefreshTokenExpirationDate(jwtBearerSettingsOptions.Value);
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

        var resetPasswordPage = await urlGeneratorService.GetPageUrlAsync("/Accounts/ResetPassword", new { secret, token });

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

        if (user is null || await AuthenticatorKeyExistsAsync(user))
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
            if (!await userManager.IsEmailConfirmedAsync(user))
            {
                return Result.Fail(FailureReasons.ClientError, "You have to confirm your account first");
            }

            if (await userManager.IsLockedOutAsync(user))
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

        if (user.IsPersistent != request.IsPersistent)
        {
            user.IsPersistent = request.IsPersistent;
            await userManager.UpdateAsync(user);
        }

        var claims = await GetClaimsAsync(user);
        return await CreateTokenAsync(user, claims);
    }

    public async Task<Result> LogoutAsync()
    {
        await signInManager.SignOutAsync();
        return Result.Ok();
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var user = await jwtBearerService.ValidateTokenAsync(request.AccessToken, true);
        if (user is null)
        {
            return Result.Fail(FailureReasons.ClientError);
        }

        var userId = user.GetClaimValue(ClaimTypes.NameIdentifier);
        var dbUser = await userManager.FindByNameAsync(userId);

        var refreshToken = await userManager.GetAuthenticationTokenAsync(dbUser, applicationName, RefreshTokenKey);
        var expirationDate = DateTime.Parse(await userManager.GetAuthenticationTokenAsync(dbUser, applicationName, RefreshTokenExpirationKey));

        if (refreshToken is null || expirationDate < DateTime.UtcNow || refreshToken != request.RefreshToken)
        {
            return Result.Fail(FailureReasons.ClientError);
        }

        var claims = user.Claims.ToList();
        await ReplaceSecurityStampClaimAsync(dbUser, claims);

        return await CreateTokenAsync(dbUser, claims);
    }

    public async Task<Result> RegisterAsync(RegisterRequest request)
    {
        var user = mapper.Map<ApplicationUser>(request);
        var result = await RegisterAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var detail = string.Join(',', result.Errors.Select(e => e.Description));
            return Result.Fail(FailureReasons.ClientError, "Couldn't registrate", detail);
        }

        var secret = await dataProtectionService.ProtectAsync(user.Id.ToString(), TimeSpan.FromMinutes(15));
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        var verifyEmailPage = await urlGeneratorService.GetPageUrlAsync("/Accounts/VerifyEmail", new { secret, token });

        var message = $$"""
            Good evening,

            Welcome to our website.
            Before you can continue, we need you to verify your account first.

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
        if (await userManager.VerifyTwoFactorTokenAsync(user, tokenProvider, request.Code))
        {
            var claims = await GetClaimsAsync(user);
            return await CreateTokenAsync(user, claims);
        }

        return Result.Fail(FailureReasons.ClientError, "Invalid two-factor code");
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

    private async Task<bool> AuthenticationTokenExistsAsync(ApplicationUser user, string tokenName)
    {
        var token = await userManager.GetAuthenticationTokenAsync(user, applicationName, tokenName);
        return token.HasValue();
    }

    private async Task<bool> AuthenticatorKeyExistsAsync(ApplicationUser user)
    {
        var secret = await userManager.GetAuthenticatorKeyAsync(user);
        return secret.HasValue();
    }

    private async Task<AuthResponse> CreateTokenAsync(ApplicationUser user, IList<Claim> claims)
    {
        var accessToken = await jwtBearerService.CreateTokenAsync(user.UserName, claims);
        var refreshToken = GenerateRefreshToken(out var expirationDate);

        if (user.IsPersistent)
        {
            await PersistTokenAsync(accessToken);
        }

        await SaveRefreshTokenAsync(user, refreshToken, expirationDate);
        return new AuthResponse(accessToken, refreshToken);
    }

    private string GenerateRefreshToken(out DateTime expirationDate)
    {
        using var generator = RandomNumberGenerator.Create();
        var randomNumber = new byte[256];

        generator.GetBytes(randomNumber);
        expirationDate = DateTime.UtcNow.Add(refreshTokenExpirationTime);

        return Convert.ToBase64String(randomNumber);
    }

    private async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user)
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

        return claims.ToList();
    }

    private async Task PersistTokenAsync(string accessToken)
    {
        var user = await jwtBearerService.ValidateTokenAsync(accessToken, true);
        var properties = new AuthenticationProperties { IsPersistent = true };

        var scheme = CookieAuthenticationDefaults.AuthenticationScheme;
        await signInManager.Context.SignInAsync(scheme, user, properties);
    }

    private async Task<IdentityResult> RegisterAsync(ApplicationUser user, string password)
    {
        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            result = await userManager.AddToRoleAsync(user, RoleNames.User);
        }

        return result;
    }

    private async Task ReplaceSecurityStampClaimAsync(ApplicationUser user, IList<Claim> claims)
    {
        var securityStampClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber);
        claims.Remove(securityStampClaim);

        await userManager.UpdateSecurityStampAsync(user);
        claims.Add(new Claim(ClaimTypes.SerialNumber, user.SecurityStamp));
    }

    private async Task<string> ResetAndGetAuthenticatorKeyAsync(ApplicationUser user)
    {
        await userManager.ResetAuthenticatorKeyAsync(user);
        return await userManager.GetAuthenticatorKeyAsync(user);
    }

    private async Task SaveRefreshTokenAsync(ApplicationUser user, string refreshToken, DateTime expirationDate)
    {
        // if exists it deletes the first refresh token saved
        if (await AuthenticationTokenExistsAsync(user, RefreshTokenKey))
        {
            await userManager.RemoveAuthenticationTokenAsync(user, applicationName, "RefreshToken");
        }

        //if exists it deletes the first refresh token expiration date saved
        if (await AuthenticationTokenExistsAsync(user, RefreshTokenExpirationKey))
        {
            await userManager.RemoveAuthenticationTokenAsync(user, applicationName, "RefreshTokenExpirationDate");
        }

        await userManager.SetAuthenticationTokenAsync(user, applicationName, "RefreshToken", refreshToken);
        await userManager.SetAuthenticationTokenAsync(user, applicationName, "RefreshTokenExpirationDate", expirationDate.ToString());
    }

    private void GetApplicationName(AppSettings appSettings)
    {
        applicationName = appSettings.ApplicationName;
    }

    private void GetRefreshTokenExpirationDate(JwtBearerSettings jwtBearerSettings)
    {
        refreshTokenExpirationTime = jwtBearerSettings.RefreshTokenExpirationTime;
    }
}