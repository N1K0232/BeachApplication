namespace BeachApplication.Shared.Models.Requests;

public record class SaveProductRequest(string Category, string Name, string Description, int? Quantity, decimal Price);