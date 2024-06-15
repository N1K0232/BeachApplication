namespace BeachApplication.Shared.Models.Requests;

public record class RegisterRequest(string FirstName, string LastName, string Email, string Password);