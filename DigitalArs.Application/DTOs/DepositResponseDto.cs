namespace DigitalArs.Application.DTOs;

/// <summary>
/// Datos que el servidor devuelve tras un depósito exitoso.
/// </summary>
public class DepositResponseDto
{
    /// <summary>Nuevo saldo de la cuenta tras el depósito.</summary>
    public decimal NewBalance { get; set; }

    /// <summary>ID del registro de Transaction creado.</summary>
    public int TransactionId { get; set; }

    /// <summary>Fecha y hora (UTC) en que se realizó el depósito.</summary>
    public DateTime Date { get; set; }
}
