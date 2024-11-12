namespace BeachApplication.Shared.Models.Requests;

public record class VerifyEmailRequest(string Secret, string Token);