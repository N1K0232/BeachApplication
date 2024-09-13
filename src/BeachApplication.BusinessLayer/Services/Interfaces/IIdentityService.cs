using BeachApplication.Shared.Models.Requests;
using BeachApplication.Shared.Models.Responses;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services.Interfaces;

public interface IIdentityService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);

    Task<Result> RegisterAsync(RegisterRequest request);

    Task<Result<ResetPasswordResponse>> ResetPasswordAsync(ResetPasswordRequest request);

    Task<Result> UpdatePasswordAsync(ChangePasswordRequest request);

    Task<Result> VerifyEmailAsync(Guid userId, string token);
}