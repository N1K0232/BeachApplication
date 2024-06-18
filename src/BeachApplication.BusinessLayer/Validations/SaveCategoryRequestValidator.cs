using BeachApplication.BusinessLayer.Resources;
using BeachApplication.Shared.Models.Requests;
using FluentValidation;

namespace BeachApplication.BusinessLayer.Validations;

public class SaveCategoryRequestValidator : AbstractValidator<SaveCategoryRequest>
{
    public SaveCategoryRequestValidator()
    {
        RuleFor(c => c.Name)
            .MaximumLength(256)
            .WithName(PropertyNames.CategoryName)
            .WithMessage(ErrorMessages.MaximumLength)
            .NotEmpty()
            .WithMessage(ErrorMessages.FieldRequired);

        RuleFor(c => c.Description)
            .MaximumLength(512)
            .WithName(PropertyNames.CategoryDescription)
            .WithMessage(ErrorMessages.MaximumLength)
            .NotEmpty()
            .WithMessage(ErrorMessages.FieldRequired);
    }
}