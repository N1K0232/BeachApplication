namespace BeachApplication.Shared.Models.Requests;

public record class LoginRequest(string Email, string Password, bool IsPersistent);