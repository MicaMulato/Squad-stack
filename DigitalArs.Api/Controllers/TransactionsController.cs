using DigitalArs.Application.DTOs;
using DigitalArs.Application.DTOs.Common;
using DigitalArs.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DigitalArs.Api.Controllers;

/// <summary>
/// Endpoints para operaciones de transacciones entre cuentas.
/// </summary>
[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    /// <summary>
    /// Transfiere fondos desde la cuenta del usuario autenticado a otra cuenta.
    /// </summary>
    /// <remarks>
    /// POST /api/transactions/transfer
    ///
    /// Body de ejemplo:
    ///
    ///     { "destinationAccountId": 2, "amount": 1000 }
    ///
    /// Reglas:
    /// - amount debe ser mayor a 0.
    /// - La cuenta destino debe existir y no estar bloqueada.
    /// - No se puede transferir a la propia cuenta.
    /// - El saldo disponible debe ser mayor o igual al monto.
    /// </remarks>
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(TransferResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Transfer([FromBody] TransferRequestDto dto)
    {
        if (dto.Amount <= 0)
            return BadRequest(new { error = "El monto debe ser mayor a 0." });

        // TODO: reemplazar por claim del JWT cuando se implemente [Authorize]
        int userId = 1;

        try
        {
            var result = await _transactionService.TransferAsync(
                userId, dto.DestinationAccountId, dto.Amount);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Devuelve el historial de movimientos del usuario autenticado con filtros y paginación.
    /// </summary>
    /// <remarks>
    /// GET /api/transactions/me
    ///
    /// Ejemplos de uso:
    ///
    ///     GET /api/transactions/me
    ///     GET /api/transactions/me?page=2&amp;pageSize=5
    ///     GET /api/transactions/me?type=1
    ///     GET /api/transactions/me?dateFrom=2026-01-01&amp;dateTo=2026-12-31
    ///     GET /api/transactions/me?amountMin=100&amp;amountMax=5000
    ///
    /// Tipos de transacción: 1=Deposit, 2=TransferIn, 3=TransferOut
    /// </remarks>
    /// <param name="query">Filtros y parámetros de paginación desde la query string.</param>
    [HttpGet("me")]
    [ProducesResponseType(typeof(PagedResultDto<TransactionItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyHistory([FromQuery] TransactionQueryDto query)
    {
        // Validaciones de formato de paginación
        if (query.Page <= 0)
            return BadRequest(new { error = "El número de página debe ser mayor a 0." });

        if (query.PageSize <= 0)
            return BadRequest(new { error = "El tamaño de página debe ser mayor a 0." });

        // TODO: reemplazar por claim del JWT cuando se implemente [Authorize]
        int userId = 1;

        try
        {
            var result = await _transactionService.GetHistoryAsync(userId, query);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
