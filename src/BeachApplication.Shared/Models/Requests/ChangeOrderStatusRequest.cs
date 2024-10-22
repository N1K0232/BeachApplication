using BeachApplication.Shared.Enums;

namespace BeachApplication.Shared.Models.Requests;

public record class ChangeOrderStatusRequest(Guid Id, OrderStatus Status);