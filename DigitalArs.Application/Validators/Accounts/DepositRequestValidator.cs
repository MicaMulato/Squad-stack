using DigitalArs.Application.DTOs.Accounts;
using FluentValidation;

namespace DigitalArs.Application.Validators.Accounts;

public class DepositRequestValidator : AbstractValidator<DepositRequest>
{
    public DepositRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a cero.")
            .LessThanOrEqualTo(ValidationConstants.MaxDepositPerOperation)
                .WithMessage($"El monto supera el limite maximo por operacion ({ValidationConstants.MaxDepositPerOperation:C}).");
    }
}
