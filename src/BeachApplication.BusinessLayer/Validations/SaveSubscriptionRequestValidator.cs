using BeachApplication.Shared.Models.Requests;
using FluentValidation;
using TinyHelpers.Extensions;

namespace BeachApplication.BusinessLayer.Validations;

public class SaveSubscriptionRequestValidator : AbstractValidator<SaveSubscriptionRequest>
{
    public SaveSubscriptionRequestValidator()
    {
        RuleFor(s => s.StartDate)
            .GreaterThan(DateTime.Today.ToDateOnly())
            .WithMessage("Insert a valid date");

        RuleFor(s => s.FinishDate)
            .GreaterThan(s => s.StartDate)
            .WithMessage("Insert a valid date");

        RuleFor(s => s.Price)
            .PrecisionScale(8, 2, true)
            .WithMessage("Insert a valid price");
    }
}