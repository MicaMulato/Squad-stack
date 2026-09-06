using DigitalArs.Api.Extensions;
using DigitalArs.Application.DTOs;
using DigitalArs.Application.DTOs.Accounts;
using DigitalArs.Application.DTOs.Common;
using DigitalArs.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalArs.Api.Controllers;

/// <summary>
/// Endpoints para operaciones sobre cuentas bancarias.
/// </summary>
[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    /// <summary>
    /// Obtiene la información y saldo de la cuenta del usuario autenticado (HU-14).
    /// </summary>
    /// <returns>Datos de la cuenta bancaria del usuario.</returns>
    /// <response code="200">Datos de la cuenta obtenidos exitosamente.</response>
    /// <response code="401">Usuario no autenticado.</response>
    /// <response code="404">No se encontró una cuenta asociada al usuario.</response>
    [HttpGet("me")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountResponse>> GetMyAccount(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var account = await _accountService.GetAccountByUserIdAsync(userId, cancellationToken);
        if (account is null)
        {
            return NotFound(new ErrorResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "No se encontró una cuenta asociada al usuario.",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return Ok(account);
    }

    /// <summary>
    /// Obtiene una cuenta por su ID (solo administradores) (HU-14).
    /// </summary>
    /// <param name="id">ID de la cuenta a consultar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Datos de la cuenta bancaria.</returns>
    /// <response code="200">Datos de la cuenta encontrados.</response>
    /// <response code="401">Usuario no autenticado.</response>
    /// <response code="403">No tiene permisos de administrador.</response>
    /// <response code="404">No se encontró la cuenta con el ID especificado.</response>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountResponse>> GetAccountById(int id, CancellationToken cancellationToken)
    {
        var account = await _accountService.GetAccountByIdAsync(id, cancellationToken);
        if (account is null)
        {
            return NotFound(new ErrorResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = $"No se encontró la cuenta con ID: {id}",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return Ok(account);
    }

    /// <summary>
    /// Deposita fondos en la cuenta del usuario (HU-15).
    /// </summary>
    /// <remarks>
    /// POST /api/accounts/deposit
    ///
    /// Body de ejemplo:
    ///
    ///     { "amount": 5000 }
    ///
    /// Reglas:
    /// - amount debe ser mayor a 0.
    /// - amount no puede superar el límite configurado en DepositSettings.MaxAmountPerOperation.
    /// - La cuenta no puede estar bloqueada.
    /// </remarks>
    /// <param name="dto">DTO con el monto a depositar.</param>
    /// <returns>Nuevo saldo, ID de transacción y fecha del depósito.</returns>
    /// <response code="200">Depósito realizado correctamente.</response>
    /// <response code="400">Monto inválido, límite superado o cuenta bloqueada.</response>
    /// <response code="404">No se encontró cuenta para el usuario.</response>
    /// <response code="500">Error interno en el servidor.</response>
    [HttpPost("deposit")]
    [ProducesResponseType(typeof(DepositResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Deposit([FromBody] DepositRequestDto dto)
    {
        // Validación: amount > 0
        if (dto.Amount <= 0)
            return BadRequest(new ErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "El monto debe ser mayor a 0.",
                TraceId = HttpContext.TraceIdentifier
            });

        var userId = User.GetUserId();

        try
        {
            var result = await _accountService.DepositAsync(userId, dto.Amount);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }
}
