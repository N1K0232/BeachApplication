using AutoMapper;
using BeachApplication.Authentication.Entities;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Contracts;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using BeachApplication.StorageProviders;
using Microsoft.AspNetCore.Identity;
using MimeMapping;
using OperationResults;
using TinyHelpers.Extensions;

namespace BeachApplication.BusinessLayer.Services;

public class MeService : IMeService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IUserService userService;
    private readonly IStorageProvider storageProvider;
    private readonly IMapper mapper;

    public MeService(UserManager<ApplicationUser> userManager,
        IUserService userService,
        IStorageProvider storageProvider,
        IMapper mapper)
    {
        this.userManager = userManager;
        this.userService = userService;
        this.storageProvider = storageProvider;
        this.mapper = mapper;
    }

    public async Task<Result> ChangePhoneNumberAsync(ChangePhoneNumberRequest request)
    {
        await Task.Delay(50);
        return Result.Ok();
    }

    public async Task<Result> DeleteProfilePhotoAsync()
    {
        var userName = await userService.GetUserNameAsync();
        var user = await userManager.FindByNameAsync(userName);

        var path = user.ProfilePhotoPath;
        if (string.IsNullOrWhiteSpace(path))
        {
            return Result.Fail(FailureReasons.ClientError, "No path was specified");
        }

        await storageProvider.DeleteAsync(path);
        user.ProfilePhotoPath = null;

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
        var userName = await userService.GetUserNameAsync();
        var user = await userManager.FindByNameAsync(userName);
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
        var userName = await userService.GetUserNameAsync();
        var user = await userManager.FindByNameAsync(userName);

        return mapper.Map<User>(user);
    }

    public async Task<Result<StreamFileContent>> GetProfilePhotoAsync()
    {
        var userName = await userService.GetUserNameAsync();
        var user = await userManager.FindByNameAsync(userName);

        if (user.ProfilePhotoPath.HasValue())
        {
            var stream = await storageProvider.ReadAsStreamAsync(user.ProfilePhotoPath);
            return new StreamFileContent(stream, MimeUtility.GetMimeMapping(user.ProfilePhotoPath));
        }

        return Result.Fail(FailureReasons.ClientError, "No path was specified");
    }

    public async Task<Result> UpdateProfilePhotoAsync(Stream stream, string fileName)
    {
        var userName = await userService.GetUserNameAsync();
        var user = await userManager.FindByNameAsync(userName);

        var path = $"users\\{user.Id}\\{fileName}";
        await storageProvider.SaveAsync(stream, path);

        user.ProfilePhotoPath = path;
        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(',', result.Errors.Select(e => e.Description));
            return Result.Fail(FailureReasons.ClientError, errors);
        }

        return Result.Ok();
    }
}