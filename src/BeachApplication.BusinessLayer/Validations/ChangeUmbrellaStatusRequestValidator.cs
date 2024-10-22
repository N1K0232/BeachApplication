using BeachApplication.Shared.Models.Requests;
using FluentValidation;

namespace BeachApplication.BusinessLayer.Validations;

public class ChangeUmbrellaStatusRequestValidator : AbstractValidator<ChangeUmbrellaStatusRequest>
{
    public ChangeUmbrellaStatusRequestValidator()
    {
        RuleFor(r => r.Id)
            .NotEmpty()
            .WithMessage("The Id is required");

        RuleFor(r => r.IsBusy)
            .NotEmpty()
            .WithMessage("The status is required");
    }
}