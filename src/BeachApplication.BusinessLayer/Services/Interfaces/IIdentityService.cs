using BeachApplication.Shared.Models.Requests;
using BeachApplication.Shared.Models.Responses;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services.Interfaces;

public interface IIdentityService
{
    Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request);

    Task<Result<ByteArrayFileContent>> GetQrCodeAsync(string token);

    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);

    Task<Result> LogoutAsync();

    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);

    Task<Result> RegisterAsync(RegisterRequest request);

    Task<Result> ResetPasswordAsync(ResetPasswordRequest request);

    Task<Result<AuthResponse>> ValidateAsync(TwoFactorValidationRequest request);

    Task<Result> VerifyEmailAsync(VerifyEmailRequest request);
}