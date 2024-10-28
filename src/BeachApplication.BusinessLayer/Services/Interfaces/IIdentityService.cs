using BeachApplication.Shared.Models.Requests;
using BeachApplication.Shared.Models.Responses;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services.Interfaces;

public interface IIdentityService
{
    Task<Result<ByteArrayFileContent>> GetQrCodeAsync(string token);

    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);

    Task<Result> RegisterAsync(RegisterRequest request);

    Task<Result<AuthResponse>> ValidateAsync(TwoFactorValidationRequest request);

    Task<Result> VerifyEmailAsync(string userId, string token);
}