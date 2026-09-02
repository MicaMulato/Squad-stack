namespace DigitalArs.Application.DTOs;

/// <summary>
/// Datos que el servidor devuelve tras una transferencia exitosa.
/// </summary>
public class TransferResponseDto
{
    /// <summary>ID del registro TransferOut creado en la cuenta origen.</summary>
    public int TransferOutId { get; set; }

    /// <summary>ID del registro TransferIn creado en la cuenta destino.</summary>
    public int TransferInId { get; set; }

    /// <summary>Monto transferido.</summary>
    public decimal Amount { get; set; }

    /// <summary>Nuevo saldo de la cuenta origen tras la transferencia.</summary>
    public decimal NewBalance { get; set; }

    /// <summary>Fecha y hora (UTC) en que se realizó la transferencia.</summary>
    public DateTime Date { get; set; }
}
