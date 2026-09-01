using DigitalArs.Application.DTOs.Transactions;
using FluentValidation;

namespace DigitalArs.Application.Validators.Transactions;

public class TransferRequestValidator : AbstractValidator<TransferRequest>
{
    public TransferRequestValidator()
    {
        RuleFor(x => x.DestinationAccountId)
            .GreaterThan(0).WithMessage("La cuenta destino es obligatoria.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a cero.");

        RuleFor(x => x.Concept)
            .MaximumLength(ValidationConstants.ConceptMaxLength)
                .WithMessage($"El concepto no puede superar los {ValidationConstants.ConceptMaxLength} caracteres.");

        // Nota: la validacion de saldo suficiente, cuenta destino existente/activa y
        // la prohibicion de autotransferencia (HU-16) requieren acceso a datos, por lo
        // que se resuelven en la capa de aplicacion/servicio, no en este validador.
    }
}
