using DigitalArs.Application.DTOs.FixedTermDeposits;
using FluentValidation;

namespace DigitalArs.Application.Validators.FixedTermDeposits;

public class CreateFixedTermDepositRequestValidator : AbstractValidator<CreateFixedTermDepositRequest>
{
    public CreateFixedTermDepositRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(1000m)
            .WithMessage("El monto mínimo para un plazo fijo es de $ 1.000,00.");

        RuleFor(x => x.DurationDays)
            .GreaterThanOrEqualTo(30)
            .WithMessage("El plazo mínimo de colocación es de 30 días.");
    }
}
