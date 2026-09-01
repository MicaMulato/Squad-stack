namespace DigitalArs.Application.DTOs.Auth;

/// <summary>
/// Respuesta de un login exitoso: token JWT y su expiracion (HU-10).
/// No incluye datos sensibles como el hash de la contrasena.
/// </summary>
public record LoginResponse
{
    public string Token { get; init; } = string.Empty;

    /// <summary>Fecha y hora (UTC) en la que expira el token.</summary>
    public DateTime ExpiresAt { get; init; }

    public int UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}
