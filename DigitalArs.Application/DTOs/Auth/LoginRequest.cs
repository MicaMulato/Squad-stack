namespace DigitalArs.Application.DTOs.Auth;

/// <summary>
/// Credenciales de inicio de sesion (HU-10).
/// </summary>
public record LoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
