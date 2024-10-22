namespace BeachApplication.Shared.Models.Requests;

public record class SaveReservationRequest(char Letter, int Number, DateOnly StartOn, TimeOnly StartAt, DateOnly EndsOn, TimeOnly EndsAt, string? Notes);