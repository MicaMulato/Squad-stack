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
    /// <param name="concept">Motivo o concepto de la transferencia.</param>
    Task<TransferResponseDto> TransferAsync(int sourceUserId, int destinationAccountId, decimal amount, string? concept = null);

    /// <summary>
    /// Devuelve el historial de transacciones de la cuenta del usuario, paginado y filtrado.
    /// Usa proyección para evitar cargar entidades innecesarias (sin N+1).
    /// </summary>
    /// <param name="userId">ID del usuario dueño de la cuenta.</param>
    /// <param name="query">Parámetros de paginación y filtros opcionales.</param>
    Task<PagedResultDto<TransactionItemDto>> GetHistoryAsync(int userId, TransactionQueryDto query);
}
