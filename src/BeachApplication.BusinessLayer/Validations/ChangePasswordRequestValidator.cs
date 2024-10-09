using BeachApplication.Shared.Models.Requests;
using FluentValidation;

namespace BeachApplication.BusinessLayer.Validations;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(c => c.Email)
            .EmailAddress()
            .NotEmpty()
            .WithMessage("Email is required");

        RuleFor(c => c.Password)
            .NotEmpty()
            .WithMessage("The password is required");

        RuleFor(c => c.Token)
            .NotEmpty()
            .WithMessage("The token is required");
    }
}