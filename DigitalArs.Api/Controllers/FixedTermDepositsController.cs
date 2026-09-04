using DigitalArs.Api.Extensions;
using DigitalArs.Application.DTOs.Common;
using DigitalArs.Application.DTOs.FixedTermDeposits;
using DigitalArs.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalArs.Api.Controllers;

/// <summary>
/// Endpoints para la gestión y simulación de depósitos a plazo fijo (HU-33).
/// </summary>
[ApiController]
[Route("api/fixed-deposits")]
public class FixedTermDepositsController : ControllerBase
{
    private readonly IFixedTermDepositService _depositService;

    public FixedTermDepositsController(IFixedTermDepositService depositService)
    {
        _depositService = depositService;
    }

    /// <summary>
    /// Crea un nuevo depósito a plazo fijo debitando saldo de la cuenta del usuario autenticado (HU-33).
    /// </summary>
    /// <param name="request">Monto a invertir y plazo en días (mínimo 30 días, mínimo $1.000).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Detalle del plazo fijo constituido con tasa e interés calculado.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(FixedTermDepositResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FixedTermDepositResponse>> Create(
        [FromBody] CreateFixedTermDepositRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _depositService.CreateAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetMyDeposits), new { id = result.Id }, result);
    }

    /// <summary>
    /// Obtiene la lista de depósitos a plazo fijo del usuario autenticado (HU-33).
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Listado de plazos fijos activos y finalizados.</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(IReadOnlyList<FixedTermDepositResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<FixedTermDepositResponse>>> GetMyDeposits(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _depositService.GetMyDepositsAsync(userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Simula el rendimiento de un plazo fijo según monto y días sin comprometer fondos (HU-33).
    /// </summary>
    /// <param name="request">Monto y días a simular.</param>
    /// <returns>Interés estimado, monto final a cobrar y fecha estimada de vencimiento.</returns>
    [HttpPost("simulate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SimulateFixedTermDepositResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public ActionResult<SimulateFixedTermDepositResponse> Simulate([FromBody] SimulateFixedTermDepositRequest request)
    {
        if (request.Amount <= 0 || request.DurationDays <= 0)
        {
            return BadRequest(new ErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "El monto y los días de simulación deben ser mayores a cero.",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        var result = _depositService.Simulate(request);
        return Ok(result);
    }
}
