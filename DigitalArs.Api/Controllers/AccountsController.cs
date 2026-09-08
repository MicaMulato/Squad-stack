using DigitalArs.Api.Extensions;
using DigitalArs.Application.DTOs;
using DigitalArs.Application.DTOs.Accounts;
using DigitalArs.Application.DTOs.Common;
using DigitalArs.Application.Exceptions;
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
    /// Obtiene la información, saldo, CVU y Alias de la cuenta del usuario autenticado (HU-14).
    /// </summary>
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
    /// Modifica el alias bancario de la cuenta del usuario autenticado.
    /// Valida formato y unicidad en la plataforma.
    /// </summary>
    [HttpPut("me/alias")]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccountResponse>> UpdateMyAlias([FromBody] UpdateAliasRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        try
        {
            var updatedAccount = await _accountService.UpdateAliasAsync(userId, request.Alias, cancellationToken);
            return Ok(updatedAccount);
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
        catch (ConflictException ex)
        {
            return Conflict(new ErrorResponse
            {
                StatusCode = StatusCodes.Status409Conflict,
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
    }

    /// <summary>
    /// Consulta y verifica en tiempo real los datos públicos de un destinatario antes de transferir por CVU o Alias.
    /// </summary>
    [HttpGet("lookup")]
    [ProducesResponseType(typeof(AccountLookupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountLookupResponse>> LookupAccount([FromQuery] string query, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        try
        {
            var recipient = await _accountService.LookupAccountAsync(userId, query, cancellationToken);
            return Ok(recipient);
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
        catch (InvalidOperationException ex)
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
    }

    /// <summary>
    /// Obtiene una cuenta por su ID (solo administradores) (HU-14).
    /// </summary>
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
    [HttpPost("deposit")]
    [ProducesResponseType(typeof(DepositResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Deposit([FromBody] DepositRequestDto dto)
    {
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
            var result = await _accountService.DepositAsync(userId, dto.Amount, dto.Concept);
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
