namespace BeachApplication.Shared.Models.Requests;

public record class SaveOrderRequest(Guid Id, Guid ProductId, int Quantity, string[]? Annotations);