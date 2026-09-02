namespace DigitalArs.Application.DTOs.Transactions;

/// <summary>
/// Transferencia de dinero a otra cuenta de DigitalArs (HU-16).
/// El backend valida saldo suficiente, existencia y estado de la cuenta destino,
/// y prohibe la autotransferencia. La operacion es atomica (Unit of Work).
/// </summary>
public record TransferRequest
{
    public int DestinationAccountId { get; init; }
    public decimal Amount { get; init; }

    /// <summary>Concepto o descripcion opcional de la transferencia.</summary>
    public string? Concept { get; init; }
}
