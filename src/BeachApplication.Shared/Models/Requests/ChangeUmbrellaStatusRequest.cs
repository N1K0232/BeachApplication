namespace BeachApplication.Shared.Models.Requests;

public record class ChangeUmbrellaStatusRequest(Guid Id, bool IsBusy);