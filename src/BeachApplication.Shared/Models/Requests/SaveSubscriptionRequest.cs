using BeachApplication.Shared.Enums;

namespace BeachApplication.Shared.Models.Requests;

public record class SaveSubscriptionRequest(DateOnly StartDate, DateOnly FinishDate, SubscriptionStatus? Status, decimal Price, string? Notes);