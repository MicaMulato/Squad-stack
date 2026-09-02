using DigitalArs.Application.DTOs.Users;
using FluentValidation;

namespace DigitalArs.Application.Validators.Users;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(ValidationConstants.NameMaxLength);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es obligatorio.")
            .MaximumLength(ValidationConstants.NameMaxLength);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El email no tiene un formato valido.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("El rol es obligatorio.")
            .Must(r => ValidationConstants.ValidRoles.Contains(r))
                .WithMessage("El rol debe ser 'Admin' o 'User'.");
    }
}
