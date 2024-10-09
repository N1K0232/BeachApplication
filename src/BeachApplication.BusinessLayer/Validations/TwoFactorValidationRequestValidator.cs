using BeachApplication.Shared.Models.Requests;
using FluentValidation;

namespace BeachApplication.BusinessLayer.Validations;

public class TwoFactorValidationRequestValidator : AbstractValidator<TwoFactorValidationRequest>
{
    public TwoFactorValidationRequestValidator()
    {
        RuleFor(r => r.Token)
            .NotEmpty()
            .WithMessage("Token is required");

        RuleFor(r => r.Code)
            .NotEmpty()
            .WithMessage("Code is required");
    }
}