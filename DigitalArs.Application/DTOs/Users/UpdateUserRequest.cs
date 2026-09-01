namespace DigitalArs.Application.DTOs.Users;

/// <summary>
/// Actualizacion de datos de un usuario por parte de un administrador (HU-12).
/// </summary>
public record UpdateUserRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;

    /// <summary>Rol asignado: "Admin" o "User".</summary>
    public string Role { get; init; } = "User";
}
