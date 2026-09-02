namespace DigitalArs.Application.DTOs.Users;

/// <summary>
/// Item de fila en el listado paginado de usuarios (HU-12).
/// Incluye el saldo para mostrarlo en la tabla de administracion sin una consulta extra.
/// </summary>
public record UserListItemResponse
{
    public int Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public decimal Balance { get; init; }
    public DateTime CreatedAt { get; init; }
}
