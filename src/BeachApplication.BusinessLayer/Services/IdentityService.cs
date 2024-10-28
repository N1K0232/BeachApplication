using System.Net.Mime;
using System.Security.Claims;
using AutoMapper;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Contracts;
using BeachApplication.DataAccessLayer;
using BeachApplication.DataAccessLayer.DataProtection;
using BeachApplication.DataAccessLayer.Entities.Identity;
using BeachApplication.Shared.Models.Requests;
using BeachApplication.Shared.Models.Responses;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using OperationResults;
using SimpleAuthentication.JwtBearer;
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

    public async Task<Result<ByteArrayFileContent>> GetQrCodeAsync(string token)
    {
        ApplicationUser user;

        try
        {
            user = await GetUserAsync(token);
        }
        catch
        {
            return Result.Fail(FailureReasons.ClientError);
        }

        if (user is null || (await userManager.GetAuthenticatorKeyAsync(user)).HasValue())
        {
            return Result.Fail(FailureReasons.ClientError);
        }

        await userManager.ResetAuthenticatorKeyAsync(user);
        var secret = await userManager.GetAuthenticatorKeyAsync(user);

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

        var sendResult = await SendVerificationEmailAsync(user);
        if (!sendResult.Successful)
        {
            var detail = string.Join(',', sendResult.ErrorMessages);
            return Result.Fail(FailureReasons.ClientError, "Couldn't send the email", detail);
        }

        return Result.Ok();
    }



    public async Task<Result<AuthResponse>> ValidateAsync(TwoFactorValidationRequest request)
    {
        ApplicationUser user;

        try
        {
            user = await GetUserAsync(request.Token);
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

    public async Task<Result> VerifyEmailAsync(string userId, string token)
    {
        var user = await userManager.FindByIdAsync(userId);
        var result = await userManager.ConfirmEmailAsync(user, token);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, RoleNames.User);
            return Result.Ok();
        }

        return Result.Fail(FailureReasons.ClientError, "Couldn't verify your email");
    }

    private async Task<ApplicationUser> GetUserAsync(string token)
    {
        var userId = await dataProtectionService.UnprotectAsync(token);
        return await userManager.FindByIdAsync(userId);
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
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.SerialNumber, user.SecurityStamp)
        }
        .Union(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        var accessToken = await jwtBearerService.CreateTokenAsync(user.UserName, claims.ToList());
        return new AuthResponse(accessToken);
    }

    private async Task<SendResponse> SendVerificationEmailAsync(ApplicationUser user)
    {
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var httpContext = httpContextAccessor.HttpContext;

        var scheme = httpContext.Request.Scheme;
        var values = new RouteValueDictionary
        {
            ["userId"] = user.Id,
            ["token"] = token
        };

        var endpoint = linkGenerator.GetUriByRouteValues(httpContextAccessor.HttpContext, "verifyemail", values, scheme);
        var sendResult = await fluentEmail.To(user.Email).Subject("Confirm your email")
            .Body($"Please confirm your email by clicking this link: <a href='{endpoint}'>Confirm Email</a>", true)
            .SendAsync();

        return sendResult;
    }
}