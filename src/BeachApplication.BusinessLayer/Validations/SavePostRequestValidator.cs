using BeachApplication.Shared.Models.Requests;
using FluentValidation;

namespace BeachApplication.BusinessLayer.Validations;

public class SavePostRequestValidator : AbstractValidator<SavePostRequest>
{
    public SavePostRequestValidator()
    {
        RuleFor(p => p.Title)
            .NotEmpty()
            .MaximumLength(256)
            .WithMessage("The title is required");

        RuleFor(p => p.Content)
            .NotEmpty()
            .WithMessage("The content is required");
    }
}