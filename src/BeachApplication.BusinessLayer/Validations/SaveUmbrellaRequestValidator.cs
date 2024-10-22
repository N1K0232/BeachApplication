using BeachApplication.Shared.Models.Requests;
using FluentValidation;

namespace BeachApplication.BusinessLayer.Validations;

public class SaveUmbrellaRequestValidator : AbstractValidator<SaveUmbrellaRequest>
{
    public SaveUmbrellaRequestValidator()
    {
        RuleFor(r => r.Letter)
            .NotEmpty()
            .WithMessage("The letter is required");

        RuleFor(r => r.Number)
            .NotEmpty()
            .WithMessage("The Number is required")
            .GreaterThan(0)
            .WithMessage("The Numbere must be greater than 0");
    }
}