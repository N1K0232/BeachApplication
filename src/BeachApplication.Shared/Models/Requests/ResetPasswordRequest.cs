namespace BeachApplication.Shared.Models.Requests;

public record class ResetPasswordRequest(string Secret, string NewPassword, string Token);