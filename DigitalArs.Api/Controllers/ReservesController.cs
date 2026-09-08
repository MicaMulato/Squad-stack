using DigitalArs.Api.Extensions;
using DigitalArs.Application.DTOs.Common;
using DigitalArs.Application.DTOs.Reserves;
using DigitalArs.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalArs.Api.Controllers;

/// <summary>
/// Endpoints para la gestión de Reservas de Dinero ("Apartados" / "Pockets").
/// </summary>
[ApiController]
[Route("api/reserves")]
[Authorize]
public class ReservesController : ControllerBase
{
    private readonly IMoneyReserveService _moneyReserveService;

    public ReservesController(IMoneyReserveService moneyReserveService)
    {
        _moneyReserveService = moneyReserveService;
    }

    /// <summary>
    /// Obtiene todas las reservas activas del usuario autenticado.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MoneyReserveResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MoneyReserveResponse>>> GetMyReserves(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var reserves = await _moneyReserveService.GetReservesAsync(userId, cancellationToken);
        return Ok(reserves);
    }

    /// <summary>
    /// Crea una nueva reserva de dinero.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(MoneyReserveResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MoneyReserveResponse>> CreateReserve(
        [FromBody] CreateReserveRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        try
        {
            var result = await _moneyReserveService.CreateReserveAsync(userId, request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Transfiere dinero desde la cuenta corriente principal hacia la reserva.
    /// </summary>
    [HttpPost("{id:int}/deposit")]
    [ProducesResponseType(typeof(MoneyReserveResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MoneyReserveResponse>> DepositIntoReserve(
        [FromRoute] int id,
        [FromBody] ReserveOperationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        try
        {
            var result = await _moneyReserveService.DepositIntoReserveAsync(userId, id, request.Amount, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Retira / libera dinero de la reserva hacia la cuenta corriente principal.
    /// </summary>
    [HttpPost("{id:int}/withdraw")]
    [ProducesResponseType(typeof(MoneyReserveResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MoneyReserveResponse>> WithdrawFromReserve(
        [FromRoute] int id,
        [FromBody] ReserveOperationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        try
        {
            var result = await _moneyReserveService.WithdrawFromReserveAsync(userId, id, request.Amount, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Elimina una reserva, reintegrando automáticamente cualquier saldo restante a la cuenta corriente.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReserve(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        try
        {
            await _moneyReserveService.DeleteReserveAsync(userId, id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }
}
