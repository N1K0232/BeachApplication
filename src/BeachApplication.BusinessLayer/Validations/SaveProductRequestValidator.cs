using BeachApplication.BusinessLayer.Resources;
using BeachApplication.Shared.Models.Requests;
using FluentValidation;

namespace BeachApplication.BusinessLayer.Validations;

public class SaveProductRequestValidator : AbstractValidator<SaveProductRequest>
{
    public SaveProductRequestValidator()
    {
        RuleFor(p => p.CategoryId)
            .NotEmpty()
            .WithName(EntityNames.Category)
            .WithMessage(ErrorMessages.FieldRequired);

        RuleFor(p => p.Name)
            .MaximumLength(256)
            .WithName(PropertyNames.ProductName)
            .WithMessage(ErrorMessages.MaximumLength)
            .NotEmpty()
            .WithMessage(ErrorMessages.FieldRequired);

        RuleFor(p => p.Description)
            .MaximumLength(4096)
            .WithName(PropertyNames.ProductDescription)
            .WithMessage(ErrorMessages.MaximumLength)
            .NotEmpty()
            .WithMessage(ErrorMessages.FieldRequired);

        RuleFor(p => p.Price)
            .PrecisionScale(8, 2, true)
            .WithName(PropertyNames.Price)
            .WithMessage(ErrorMessages.InvalidPrice)
            .NotEmpty()
            .WithMessage(ErrorMessages.FieldRequired);
    }
}