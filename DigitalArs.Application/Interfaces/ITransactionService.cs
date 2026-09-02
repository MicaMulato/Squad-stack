using DigitalArs.Application.DTOs;

namespace DigitalArs.Application.Interfaces;

/// <summary>
/// Contrato para las operaciones de transacciones entre cuentas.
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Transfiere fondos desde la cuenta del usuario autenticado hacia la cuenta destino.
    /// La operación es atómica: crea dos registros vinculados (TransferOut + TransferIn)
    /// y actualiza ambos saldos en una sola transacción SQL.
    /// </summary>
    /// <param name="sourceUserId">ID del usuario que origina la transferencia.</param>
    /// <param name="destinationAccountId">ID de la cuenta que recibirá los fondos.</param>
    /// <param name="amount">Monto a transferir. Debe ser mayor a 0.</param>
    /// <returns>DTO con los IDs de ambas transacciones, monto, nuevo saldo y fecha.</returns>
    Task<TransferResponseDto> TransferAsync(int sourceUserId, int destinationAccountId, decimal amount);
}
