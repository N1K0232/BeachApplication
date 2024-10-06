using System.Security.Claims;
using AutoMapper;
using BeachApplication.DataAccessLayer;
using BeachApplication.DataAccessLayer.Entities.Identity;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using BeachApplication.Shared.Models.Responses;
using FluentEmail.Core;
using Microsoft.AspNetCore.Identity;
using MinimalHelpers.Routing;
using SimpleAuthentication.JwtBearer;

namespace BeachApplication.Endpoints;

public class IdentityEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var identityApiGroup = endpoints.MapGroup("/api/auth");

        identityApiGroup.MapGet("/profile", GetMeAsync)
            .Produces<User>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization("UserActive")
            .WithName("profile")
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

        identityApiGroup.MapGet("/verifyemail", VerifyEmailAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .AllowAnonymous()
            .WithName("verifyemail")
            .WithOpenApi();
    }

    private static async Task<IResult> GetMeAsync(UserManager<ApplicationUser> userManager, IMapper mapper, ClaimsPrincipal principal)
    {
        var dbUser = await userManager.GetUserAsync(principal);
        var user = mapper.Map<User>(dbUser);

        return TypedResults.Ok(user);
    }

    private static async Task<IResult> LoginAsync(SignInManager<ApplicationUser> signInManager, IJwtBearerService jwtBearerService, LoginRequest request)
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
        }.Union(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        var accessToken = await jwtBearerService.CreateTokenAsync(user.UserName, claims.ToList());
        return TypedResults.Ok(new AuthResponse(accessToken));
    }

    private static async Task<IResult> RegisterAsync(UserManager<ApplicationUser> userManager, IFluentEmail fluentEmail, LinkGenerator linkGenerator, HttpContext httpContext, RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email,
            PhoneNumber = request.PhoneNumber
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return TypedResults.BadRequest(result.Errors.Select(e => e.Description));
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var values = new RouteValueDictionary
        {
            ["userId"] = user.Id,
            ["token"] = token
        };

        var endpoint = linkGenerator.GetUriByRouteValues(httpContext, "verifyemail", values, httpContext.Request.Scheme);
        var sendResult = await fluentEmail.To(user.Email).Subject("Confirm your email")
            .Body($"Please confirm your email by clicking this link: <a href='{endpoint}'>Confirm Email</a>", true)
            .SendAsync();

        if (!sendResult.Successful)
        {
            return TypedResults.BadRequest(sendResult.ErrorMessages);
        }

        return TypedResults.Created();
    }

    private static async Task<IResult> ResetPasswordAsync(UserManager<ApplicationUser> userManager, ResetPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return TypedResults.BadRequest("Invalid email address");
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        return TypedResults.Ok(new ResetPasswordResponse(token));
    }

    private static async Task<IResult> UpdatePasswordAsync(UserManager<ApplicationUser> userManager, ChangePasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return TypedResults.BadRequest("Invalid email address");
        }

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.Password);
        if (!result.Succeeded)
        {
            return TypedResults.BadRequest(result.Errors.Select(e => e.Description));
        }

        return TypedResults.NoContent();
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