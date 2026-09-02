using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalArs.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
    
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    // GET /api/accounts/me
    [HttpGet("me")]
    public async Task<ActionResult<AccountDto>> GetMyAccount(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
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

    // GET /api/accounts/{id}
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AccountDto>> GetAccountById(Guid id, CancellationToken cancellationToken)
    {
        var account = await _accountService.GetAccountByIdAsync(id, cancellationToken);
        if (account is null)
        {
            return NotFound($"No se encontró la cuenta con ID: {id}");
        }

        return Ok(account);
    }
}
