using BeachApplication.Shared.Models.Requests;
using FluentValidation;
using TinyHelpers.Extensions;

namespace BeachApplication.BusinessLayer.Validations;

public class SaveReservationRequestValidator : AbstractValidator<SaveReservationRequest>
{
    public SaveReservationRequestValidator()
    {
        RuleFor(r => r.StartOn)
            .GreaterThanOrEqualTo(DateTime.Today.ToDateOnly())
            .WithMessage("You can't insert past dates");

        RuleFor(r => r.StartAt)
            .GreaterThanOrEqualTo(DateTime.Today.ToTimeOnly())
            .WithMessage("You can't insert past time");

        RuleFor(r => r.EndsOn)
            .GreaterThan(r => r.StartOn)
            .WithMessage("You can't insert a date before the reservation start");

        RuleFor(r => r.EndsAt)
            .GreaterThan(r => r.StartAt)
            .WithMessage("You can't insert a time before the reservation start");
    }
}