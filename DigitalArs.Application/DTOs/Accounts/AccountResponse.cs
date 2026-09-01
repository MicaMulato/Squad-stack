namespace DigitalArs.Application.DTOs.Accounts;

/// <summary>
/// Datos de la cuenta del usuario: id, saldo y fecha de creacion (HU-14).
/// Consulta de solo lectura, proyectada desde la entidad Account.
/// </summary>
/// <remarks>
/// PENDIENTE (coordinar con la HU del modelo): la entidad Account todavia NO tiene
/// una propiedad CreatedAt. HU-14 pide "fecha de creacion". Opciones: (A) agregar
/// CreatedAt a Account con su migracion; (B) mapear el CreatedAt del User dueño.
/// El campo queda declarado en el DTO para no cambiar el contrato luego.
/// </remarks>
public record AccountResponse
{
    public int Id { get; init; }
    public decimal Balance { get; init; }
    public bool IsBlocked { get; init; }
    public DateTime CreatedAt { get; init; }
}
