namespace BeachApplication.Shared.Models.Requests;

public record class TwoFactorValidationRequest(string Token, string Code);