using DigitalArs.Application.DTOs;
using DigitalArs.Application.Interfaces;
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
    /// Deposita fondos en la cuenta del usuario.
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

        // TODO: reemplazar por claim del JWT cuando se implemente [Authorize]
        // var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        // int userId = int.Parse(userIdClaim!);
        int userId = 1; // temporario para desarrollo (ID 1 = Admin del seed)

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
