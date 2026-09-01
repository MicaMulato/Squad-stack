namespace DigitalArs.Application.DTOs.Users;

/// <summary>
/// Representacion publica de un usuario (HU-12 detalle, HU-13 /me).
/// Deliberadamente NO expone PasswordHash ni ningun dato sensible (criterio HU-08).
/// </summary>
public record UserResponse
{
    public int Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;

    /// <summary>Indica si el usuario esta activo (inverso de IsDeleted).</summary>
    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }
}
