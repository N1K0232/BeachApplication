using BeachApplication.BusinessLayer.Resources;
using BeachApplication.Shared.Models.Requests;
using FluentValidation;

namespace BeachApplication.BusinessLayer.Validations;

public class SaveOrderRequestValidator : AbstractValidator<SaveOrderRequest>
{
    public SaveOrderRequestValidator()
    {
        RuleFor(o => o.ProductId)
            .NotEmpty()
            .WithName(EntityNames.Product)
            .WithMessage(ErrorMessages.FieldRequired);

        RuleFor(o => o.Quantity)
            .GreaterThan(0)
            .WithName(PropertyNames.Quantity)
            .WithMessage(ErrorMessages.NegativeQuantity);
    }
}