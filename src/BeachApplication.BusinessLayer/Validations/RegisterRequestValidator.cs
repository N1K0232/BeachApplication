using BeachApplication.Shared.Models.Requests;
using FluentValidation;

namespace BeachApplication.BusinessLayer.Validations;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(r => r.FirstName)
            .NotEmpty()
            .WithMessage("The First Name is required");

        RuleFor(r => r.LastName)
            .NotEmpty()
            .WithMessage("The Last Name is required");

        RuleFor(r => r.Email)
            .EmailAddress()
            .NotEmpty()
            .WithMessage("The Email is required");

        RuleFor(r => r.Password)
            .NotEmpty()
            .WithMessage("The Password is required");
    }
}