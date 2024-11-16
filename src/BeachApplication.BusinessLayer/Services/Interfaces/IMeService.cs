using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services.Interfaces;

public interface IMeService
{
    Task<Result> ChangePhoneNumberAsync(ChangePhoneNumberRequest request);

    Task<Result> DeleteProfilePhotoAsync();

    Task<Result> EnableTwoFactorAsync();

    Task<Result<User>> GetAsync();

    Task<Result<StreamFileContent>> GetProfilePhotoAsync();

    Task<Result> UpdateProfilePhotoAsync(Stream stream, string fileName);
}