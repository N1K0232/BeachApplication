using BeachApplication.Shared.Models.Requests;
using FluentValidation;

namespace BeachApplication.BusinessLayer.Validations;

public class SaveCommentRequestValidator : AbstractValidator<SaveCommentRequest>
{
    public SaveCommentRequestValidator()
    {
        RuleFor(r => r.Score)
            .InclusiveBetween(1, 5)
            .WithMessage("Please insert a number between 1 and 5");

        RuleFor(r => r.Title)
            .NotEmpty()
            .WithMessage("The title is required")
            .MaximumLength(150)
            .WithMessage("The maximum length for the title is 150 characters");

        RuleFor(r => r.Text)
            .NotEmpty()
            .WithMessage("The text is required");
    }
}