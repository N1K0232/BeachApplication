﻿using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BeachApplication.Authentication;
using BeachApplication.Authentication.Entities;
using BeachApplication.Authentication.Extensions;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.BusinessLayer.Settings;
using BeachApplication.Shared.Models.Requests;
using BeachApplication.Shared.Models.Responses;
using FluentEmail.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly IFluentEmail fluentEmail;
    private readonly LinkGenerator linkGenerator;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly JwtSettings jwtSettings;

    public IdentityService(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IFluentEmail fluentEmail,
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor,
        IOptions<JwtSettings> jwtSettingsOptions)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.fluentEmail = fluentEmail;
        this.linkGenerator = linkGenerator;
        this.httpContextAccessor = httpContextAccessor;
        jwtSettings = jwtSettingsOptions.Value;
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

        var signInResult = await signInManager.PasswordSignInAsync(user, request.Password, request.IsPersistent, false);
        if (signInResult.Succeeded)
        {
            var userRoles = await userManager.GetRolesAsync(user);
            await userManager.UpdateSecurityStampAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.SerialNumber, user.SecurityStamp ?? string.Empty)
            }.Union(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            return await SaveAndGetResponseAsync(user, claims);
        }

        if (signInResult.IsLockedOut)
        {
            return Result.Fail(FailureReasons.ClientError, "you're locked out", $"you're locked out until {user.LockoutEnd}");
        }

        await userManager.AccessFailedAsync(user);
        return Result.Fail(FailureReasons.ClientError, "Login failed", "Invalid username or password");
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var user = await ValidateAsync(request.AccessToken);
        if (user is not null)
        {
            var userId = user.GetId();
            var dbUser = await userManager.FindByNameAsync(userId.ToString());

            if (dbUser?.RefreshToken == null || dbUser?.RefreshTokenExpirationDate < DateTime.UtcNow || dbUser?.RefreshToken != request.RefreshToken)
            {
                return Result.Fail(FailureReasons.ClientError, "Couldn't validate refresh token", "Couldn't validate refresh token");
            }

            return await SaveAndGetResponseAsync(dbUser, user.Claims);
        }

        return Result.Fail(FailureReasons.ClientError, "Couldn't validate access token", "Couldn't validate access token");
    }

    public async Task<Result> RegisterAsync(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email
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

    private async Task<Result<AuthResponse>> SaveAndGetResponseAsync(ApplicationUser user, IEnumerable<Claim> claims)
    {
        var securityKey = Encoding.UTF8.GetBytes(jwtSettings.SecurityKey);
        var randomNumber = new byte[4096];

        var symmetricSecurityKey = new SymmetricSecurityKey(securityKey);
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        using var generator = RandomNumberGenerator.Create();
        var jwtSecurityToken = new JwtSecurityToken
        (
            jwtSettings.Issuer,
            jwtSettings.Audience,
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials
        );

        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        generator.GetBytes(randomNumber);

        var accessToken = jwtSecurityTokenHandler.WriteToken(jwtSecurityToken);
        var refreshToken = Convert.ToBase64String(randomNumber);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpirationDate = DateTime.UtcNow.AddMinutes(jwtSettings.RefreshTokenExpirationMinutes);

        var result = await userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            return new AuthResponse(accessToken, refreshToken);
        }

        return Result.Fail(FailureReasons.ClientError, result.GetErrors());
    }

    private async Task SendVerificationEmailAsync(ApplicationUser user)
    {
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var httpContext = httpContextAccessor.HttpContext!;

        var scheme = httpContext.Request.Scheme;
        var values = new RouteValueDictionary
        {
            ["userId"] = user.Id,
            ["token"] = token
        };

        var confirmationLink = linkGenerator.GetUriByRouteValues(httpContext, "verifyemail", values, scheme);
        var response = await fluentEmail.To(user.Email)
            .Subject("Confirm your email")
            .Body($"Please confirm your email by clicking this link: <a href='{confirmationLink}'>Confirm Email</a>", true)
            .SendAsync();
    }

    private Task<ClaimsPrincipal> ValidateAsync(string accessToken)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecurityKey)),
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var user = tokenHandler.ValidateToken(accessToken, parameters, out var securityToken);
            if (securityToken is JwtSecurityToken jwtSecurityToken && jwtSecurityToken.Header.Alg == SecurityAlgorithms.HmacSha256)
            {
                return Task.FromResult<ClaimsPrincipal>(user);
            }
        }
        catch
        {
        }

        return Task.FromResult<ClaimsPrincipal>(null);
    }
}