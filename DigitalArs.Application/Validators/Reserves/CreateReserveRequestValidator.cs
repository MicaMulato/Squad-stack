using DigitalArs.Application.DTOs.Reserves;
using FluentValidation;

namespace DigitalArs.Application.Validators.Reserves;

public class CreateReserveRequestValidator : AbstractValidator<CreateReserveRequest>
{
    public CreateReserveRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre de la reserva es obligatorio.")
            .MaximumLength(50)
            .WithMessage("El nombre no puede exceder los 50 caracteres.");

        RuleFor(x => x.TargetAmount)
            .GreaterThan(0)
            .When(x => x.TargetAmount.HasValue)
            .WithMessage("La meta de ahorro debe ser mayor a cero si se especifica.");
    }
}
