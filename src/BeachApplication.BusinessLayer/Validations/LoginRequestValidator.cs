using BeachApplication.Shared.Models.Requests;
using FluentValidation;

namespace BeachApplication.BusinessLayer.Validations;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(l => l.Email)
            .EmailAddress()
            .NotEmpty()
            .WithMessage("Email is required");

        RuleFor(l => l.Password)
            .NotEmpty()
            .WithMessage("Password is required");
    }
}