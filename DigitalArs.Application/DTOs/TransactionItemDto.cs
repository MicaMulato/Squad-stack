using DigitalArs.Domain.Enums;

namespace DigitalArs.Application.DTOs;

/// <summary>
/// Representa un movimiento individual en el historial de la cuenta.
/// Es una proyección de Transaction: solo trae las columnas necesarias,
/// evitando cargar entidades de navegación (Account, User) innecesariamente.
/// </summary>
public class TransactionItemDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public string? Concept { get; set; }

    private DateTime _date;
    public DateTime Date
    {
        get => _date;
        set => _date = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    /// <summary>
    /// ID de la cuenta relacionada (destino en TransferOut, origen en TransferIn).
    /// Null en depósitos.
    /// </summary>
    public int? ToAccountId { get; set; }
}
