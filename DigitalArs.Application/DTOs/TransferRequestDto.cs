namespace DigitalArs.Application.DTOs;

/// <summary>
/// Datos que el cliente envía en el body de POST /api/transactions/transfer.
/// </summary>
public class TransferRequestDto
{
    /// <summary>ID de la cuenta destino que recibirá los fondos.</summary>
    public int DestinationAccountId { get; set; }

    /// <summary>Monto a transferir. Debe ser mayor a 0.</summary>
    public decimal Amount { get; set; }

    /// <summary>Motivo o concepto de la transferencia.</summary>
    public string? Concept { get; set; }
}

