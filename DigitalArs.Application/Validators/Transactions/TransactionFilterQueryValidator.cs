using DigitalArs.Application.DTOs.Transactions;
using FluentValidation;

namespace DigitalArs.Application.Validators.Transactions;

public class TransactionFilterQueryValidator : AbstractValidator<TransactionFilterQuery>
{
    public TransactionFilterQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("La pagina debe ser mayor a cero.");

        When(x => x.DateFrom.HasValue && x.DateTo.HasValue, () =>
        {
            RuleFor(x => x.DateFrom)
                .LessThanOrEqualTo(x => x.DateTo)
                    .WithMessage("La fecha desde no puede ser posterior a la fecha hasta.");
        });

        When(x => x.MinAmount.HasValue && x.MaxAmount.HasValue, () =>
        {
            RuleFor(x => x.MinAmount)
                .LessThanOrEqualTo(x => x.MaxAmount)
                    .WithMessage("El monto minimo no puede ser mayor al monto maximo.");
        });

        RuleFor(x => x.MinAmount)
            .GreaterThanOrEqualTo(0).When(x => x.MinAmount.HasValue)
                .WithMessage("El monto minimo no puede ser negativo.");
    }
}
