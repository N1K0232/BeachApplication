using BeachApplication.Shared.Models.Requests;
using FluentValidation;

namespace BeachApplication.BusinessLayer.Validations;

public class SaveCategoryRequestValidator : AbstractValidator<SaveCategoryRequest>
{
    public SaveCategoryRequestValidator()
    {
        RuleFor(c => c.Name)
            .MaximumLength(256)
            .NotEmpty()
            .WithMessage("the name is required");

        RuleFor(c => c.Description)
            .MaximumLength(512)
            .NotEmpty()
            .WithMessage("the description is required");
    }
}