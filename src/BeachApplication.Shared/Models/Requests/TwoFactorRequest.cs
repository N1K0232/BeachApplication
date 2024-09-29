namespace BeachApplication.Shared.Models.Requests;

public record class TwoFactorRequest(string Token, string Code);