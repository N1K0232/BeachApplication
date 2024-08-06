namespace BeachApplication.Shared.Models;

public record class User(Guid Id, string FirstName, string? LastName, string Email);