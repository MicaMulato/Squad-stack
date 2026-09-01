using DigitalArs.Application.DTOs.Auth;
using FluentValidation;

namespace DigitalArs.Application.Validators.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El email no tiene un formato valido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contrasena es obligatoria.");
    }
}
