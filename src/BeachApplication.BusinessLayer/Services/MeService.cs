using System.Net.Mime;
using AutoMapper;
using BeachApplication.Authentication.Entities;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Contracts;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Microsoft.AspNetCore.Identity;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services;

public class MeService : IMeService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IUserService userService;
    private readonly IMapper mapper;

    public MeService(UserManager<ApplicationUser> userManager,
        IUserService userService,
        IMapper mapper)
    {
        this.userManager = userManager;
        this.userService = userService;
        this.mapper = mapper;
    }

    public async Task<Result> ChangePhoneNumberAsync(ChangePhoneNumberRequest request)
    {
        await Task.Delay(50);
        return Result.Ok();
    }

    public async Task<Result> DeleteProfilePhotoAsync()
    {
        var user = await userManager.FindByNameAsync(userService.GetUserName());
        if (user.ProfilePhoto is null)
        {
            return Result.Fail(FailureReasons.ClientError, "No profile photo was found");
        }

        user.ProfilePhoto = null;
        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(',', result.Errors.Select(e => e.Description));
            return Result.Fail(FailureReasons.ClientError, errors);
        }

        return Result.Ok();
    }

    public async Task<Result> EnableTwoFactorAsync()
    {
        var user = await userManager.FindByNameAsync(userService.GetUserName());
        user.TwoFactorEnabled = true;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(',', result.Errors.Select(e => e.Description));
            return Result.Fail(FailureReasons.ClientError, errors);
        }

        return Result.Ok();
    }

    public async Task<Result<User>> GetAsync()
    {
        var dbUser = await userManager.FindByNameAsync(userService.GetUserName());
        var userRoles = await userManager.GetRolesAsync(dbUser);

        var user = mapper.Map<User>(dbUser);
        user.Role = userRoles.First();

        return user;
    }

    public async Task<Result<ByteArrayFileContent>> GetProfilePhotoAsync()
    {
        var user = await userManager.FindByNameAsync(userService.GetUserName());
        if (user.ProfilePhoto is null)
        {
            return Result.Fail(FailureReasons.ClientError, "No image was specified");
        }

        return new ByteArrayFileContent(user.ProfilePhoto, user.ContentType ?? MediaTypeNames.Image.Png);
    }

    public async Task<Result> UpdateProfilePhotoAsync(Stream stream, string contentType)
    {
        await using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);

        var user = await userManager.FindByNameAsync(userService.GetUserName());
        user.ProfilePhoto = memoryStream.ToArray();

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(',', result.Errors.Select(e => e.Description));
            return Result.Fail(FailureReasons.ClientError, errors);
        }

        return Result.Ok();
    }
}