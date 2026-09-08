using DigitalArs.Api.Extensions;
using DigitalArs.Application.DTOs.Common;
using DigitalArs.Application.DTOs.Services;
using DigitalArs.Application.Interfaces;
using DigitalArs.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalArs.Api.Controllers;

/// <summary>
/// Endpoints para la gestión y pago de servicios públicos y privados (Luz, Agua, Gas, Telco, Impuestos).
/// </summary>
[ApiController]
[Route("api/services")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly IServicePaymentService _servicePaymentService;

    public ServicesController(IServicePaymentService servicePaymentService)
    {
        _servicePaymentService = servicePaymentService;
    }

    /// <summary>
    /// Obtiene el catálogo de proveedores de servicios disponibles, opcionalmente filtrados por categoría.
    /// </summary>
    [HttpGet("providers")]
    [ProducesResponseType(typeof(IReadOnlyList<ServiceProviderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ServiceProviderDto>>> GetProviders(
        [FromQuery] ServiceCategory? category,
        CancellationToken cancellationToken)
    {
        var providers = await _servicePaymentService.GetProvidersAsync(category, cancellationToken);
        return Ok(providers);
    }

    /// <summary>
    /// Simula la consulta de una factura de un servicio ingresando el código de pago o número de cliente.
    /// </summary>
    [HttpGet("invoice/simulate")]
    [ProducesResponseType(typeof(SimulateInvoiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SimulateInvoiceResponse>> SimulateInvoice(
        [FromQuery] int providerId,
        [FromQuery] string referenceNumber,
        CancellationToken cancellationToken)
    {
        if (providerId <= 0 || string.IsNullOrWhiteSpace(referenceNumber))
        {
            return BadRequest(new ErrorResponse { Message = "Debe especificar el proveedor y el código de factura o referencia." });
        }

        try
        {
            var invoice = await _servicePaymentService.SimulateInvoiceAsync(providerId, referenceNumber.Trim(), cancellationToken);
            return Ok(invoice);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Ejecuta el pago de una factura de servicio debitando de la cuenta principal o de una reserva.
    /// </summary>
    [HttpPost("pay")]
    [ProducesResponseType(typeof(ServicePaymentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServicePaymentResponse>> PayService(
        [FromBody] ServicePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        try
        {
            var result = await _servicePaymentService.PayServiceAsync(userId, request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
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
    /// Obtiene el historial de comprobantes de pago de servicios del usuario autenticado.
    /// </summary>
    [HttpGet("my-payments")]
    [ProducesResponseType(typeof(IReadOnlyList<ServicePaymentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ServicePaymentResponse>>> GetMyPayments(
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var payments = await _servicePaymentService.GetMyPaymentsAsync(userId, cancellationToken);
        return Ok(payments);
    }
}
