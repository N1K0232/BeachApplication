namespace BeachApplication.Shared.Models.Requests;

public record class VerifyEmailRequest(string Email, string Token);