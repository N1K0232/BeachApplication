namespace BeachApplication.Shared.Models.Requests;

public record class SaveCartRequest(Guid Id, Guid ProductId, int Quantity, string Annotations);