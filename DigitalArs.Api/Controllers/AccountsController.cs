using System.Security.Claims;
using DigitalArs.Application.DTOs;
using DigitalArs.Application.DTOs.Accounts;
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
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountResponse>> GetMyAccount(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var account = await _accountService.GetAccountByUserIdAsync(userId, cancellationToken);
        if (account is null)
        {
            return NotFound("No se encontró una cuenta asociada al usuario.");
        }

        return Ok(account);
    }

    /// <summary>
    /// Obtiene una cuenta por su ID (solo administradores) (HU-14).
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountResponse>> GetAccountById(int id, CancellationToken cancellationToken)
    {
        var account = await _accountService.GetAccountByIdAsync(id, cancellationToken);
        if (account is null)
        {
            return NotFound($"No se encontró la cuenta con ID: {id}");
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
    [HttpPost("deposit")]
    [ProducesResponseType(typeof(DepositResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deposit([FromBody] DepositRequestDto dto)
    {
        // Validación: amount > 0
        if (dto.Amount <= 0)
            return BadRequest(new { error = "El monto debe ser mayor a 0." });

        // Obtener userId de claims si está autenticado, o fallback para desarrollo
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int userId = int.TryParse(userIdClaim, out var parsedId) ? parsedId : 1;

        try
        {
            var result = await _accountService.DepositAsync(userId, dto.Amount);
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
}
