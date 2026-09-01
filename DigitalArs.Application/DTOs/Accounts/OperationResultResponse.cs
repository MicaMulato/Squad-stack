namespace DigitalArs.Application.DTOs.Accounts;

/// <summary>
/// Resultado de una operacion sobre la cuenta (deposito o transferencia).
/// Devuelve el nuevo saldo y el id de la transaccion generada, para que el
/// frontend actualice el dashboard sin una consulta adicional (HU-15, HU-16, HU-25).
/// </summary>
public record OperationResultResponse
{
    public int TransactionId { get; init; }
    public decimal NewBalance { get; init; }
    public DateTime Date { get; init; }
}
