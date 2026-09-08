namespace DigitalArs.Application.DTOs.Accounts;

/// <summary>
/// Datos de la cuenta del usuario: id, saldo, estado, cvu, alias y fecha de creacion (HU-14).
/// Consulta de solo lectura, proyectada desde la entidad Account.
/// </summary>
public record AccountResponse
{
    public int Id { get; init; }
    public decimal Balance { get; init; }
    public bool IsBlocked { get; init; }
    public string Cvu { get; init; } = string.Empty;
    public string Alias { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
