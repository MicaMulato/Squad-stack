namespace DigitalArs.Application.DTOs.Users;

/// <summary>
/// Alta de usuario por parte de un administrador (HU-12).
/// El backend crea el usuario y su cuenta en la misma transaccion.
/// </summary>
public record CreateUserRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;

    /// <summary>Rol asignado: "Admin" o "User".</summary>
    public string Role { get; init; } = "User";

    /// <summary>Saldo inicial opcional de la cuenta que se crea junto al usuario.</summary>
    public decimal InitialBalance { get; init; } = 0m;
}
