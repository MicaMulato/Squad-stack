using DigitalArs.Application.DTOs.Users;
using FluentValidation;

namespace DigitalArs.Application.Validators.Users;

public class UpdateMyProfileRequestValidator : AbstractValidator<UpdateMyProfileRequest>
{
    public UpdateMyProfileRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(ValidationConstants.NameMaxLength);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es obligatorio.")
            .MaximumLength(ValidationConstants.NameMaxLength);

        // Cambio de contrasena opcional: solo se valida si se envio NewPassword.
        When(x => !string.IsNullOrEmpty(x.NewPassword), () =>
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Debe indicar la contrasena actual para cambiarla.");

            RuleFor(x => x.NewPassword)
                .MinimumLength(ValidationConstants.PasswordMinLength)
                    .WithMessage($"La nueva contrasena debe tener al menos {ValidationConstants.PasswordMinLength} caracteres.")
                .NotEqual(x => x.CurrentPassword)
                    .WithMessage("La nueva contrasena debe ser distinta de la actual.");
        });
    }
}
