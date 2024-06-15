namespace BeachApplication.Shared.Models.Requests;

public record class ChangePasswordRequest(string Email, string Password, string Token);