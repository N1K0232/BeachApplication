namespace BeachApplication.Shared.Models.Requests;

public record class SaveOrderRequest(Guid OrderId, Guid ProductId, int Quantity, string[]? Annotations);