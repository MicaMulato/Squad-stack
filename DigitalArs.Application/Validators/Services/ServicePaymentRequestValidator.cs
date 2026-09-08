using DigitalArs.Application.DTOs.Services;
using FluentValidation;

namespace DigitalArs.Application.Validators.Services;

public class ServicePaymentRequestValidator : AbstractValidator<ServicePaymentRequest>
{
    public ServicePaymentRequestValidator()
    {
        RuleFor(x => x.ServiceProviderId)
            .GreaterThan(0)
            .WithMessage("Debe seleccionar un proveedor de servicios válido.");

        RuleFor(x => x.ReferenceNumber)
            .NotEmpty()
            .WithMessage("Debe ingresar el código de pago o número de referencia/factura.")
            .MaximumLength(50)
            .WithMessage("El número de referencia no puede exceder los 50 caracteres.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("El monto a abonar debe ser mayor a cero.");
    }
}
