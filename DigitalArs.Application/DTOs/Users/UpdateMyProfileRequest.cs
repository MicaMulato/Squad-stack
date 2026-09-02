namespace DigitalArs.Application.DTOs.Users;

/// <summary>
/// Actualizacion de los datos propios del usuario logueado (HU-13).
/// El id se toma del token, no del body. No puede cambiar rol ni saldo.
/// El cambio de contrasena es opcional y exige la contrasena actual.
/// </summary>
public record UpdateMyProfileRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;

    /// <summary>Contrasena actual. Requerida solo si se quiere cambiar la contrasena.</summary>
    public string? CurrentPassword { get; init; }

    /// <summary>Nueva contrasena. Si viene, CurrentPassword es obligatoria.</summary>
    public string? NewPassword { get; init; }
}
