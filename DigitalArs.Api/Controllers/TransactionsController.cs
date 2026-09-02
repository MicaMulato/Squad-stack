using DigitalArs.Application.DTOs;
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
    /// <param name="dto">DTO con la cuenta destino y el monto a transferir.</param>
    /// <returns>IDs de ambas transacciones, monto, nuevo saldo y fecha.</returns>
    /// <response code="200">Transferencia realizada correctamente.</response>
    /// <response code="400">Monto inválido, saldo insuficiente, cuenta bloqueada o autotransferencia.</response>
    /// <response code="404">Cuenta origen o destino no encontrada.</response>
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(TransferResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Transfer([FromBody] TransferRequestDto dto)
    {
        // Validación de formato: amount > 0
        if (dto.Amount <= 0)
            return BadRequest(new { error = "El monto debe ser mayor a 0." });

        // TODO: reemplazar por claim del JWT cuando se implemente [Authorize]
        // var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        // int userId = int.Parse(userIdClaim!);
        int userId = 1; // temporario para desarrollo (ID 1 = Admin del seed)

        try
        {
            var result = await _transactionService.TransferAsync(
                userId,
                dto.DestinationAccountId,
                dto.Amount);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            // Autotransferencia
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            // Cuenta origen o destino no existe
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // Saldo insuficiente o cuenta destino bloqueada
            return BadRequest(new { error = ex.Message });
        }
    }
}
