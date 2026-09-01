using System.Security.Claims;
using DigitalArs.Application.DTOs.Common;
using DigitalArs.Application.DTOs.Users;
using DigitalArs.Application.Exceptions;
using DigitalArs.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DigitalArs.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
// Nota: [Authorize] o [Authorize(Roles = "Admin")] sera integrado en HU-10/HU-11 por el compañero asignado.
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // =========================================================================
    // HU-13: Ver y actualizar mis datos (/me)
    // =========================================================================

    /// <summary>
    /// Obtiene los datos del usuario autenticado a partir de su token (HU-13).
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetMyProfile(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(new ErrorResponse
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "No se pudo identificar al usuario autenticado.",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        var user = await _userService.GetMyProfileAsync(userId.Value, cancellationToken);
        if (user == null)
        {
            return NotFound(new ErrorResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Usuario no encontrado.",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return Ok(user);
    }

    /// <summary>
    /// Actualiza los datos del usuario autenticado (nombre, apellido y opcionalmente contraseña) (HU-13).
    /// </summary>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> UpdateMyProfile([FromBody] UpdateMyProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(new ErrorResponse
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "No se pudo identificar al usuario autenticado.",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        try
        {
            var updatedUser = await _userService.UpdateMyProfileAsync(userId.Value, request, cancellationToken);
            if (updatedUser == null)
            {
                return NotFound(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Usuario no encontrado.",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            return Ok(updatedUser);
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

    // =========================================================================
    // HU-12: CRUD Administrativo de Usuarios
    // =========================================================================

    /// <summary>
    /// Obtiene el listado paginado de usuarios con filtros por nombre, email, rol y estado activo (HU-12).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UserListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<UserListItemResponse>>> GetUsers([FromQuery] UserFilterQuery query, CancellationToken cancellationToken)
    {
        var result = await _userService.GetUsersAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el detalle de un usuario por su identificador (HU-12).
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return NotFound(new ErrorResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = $"No se encontró el usuario con ID {id}.",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return Ok(user);
    }

    /// <summary>
    /// Crea un nuevo usuario y su cuenta asociada en una misma transacción (HU-12).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var createdUser = await _userService.CreateUserAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
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
    }

    /// <summary>
    /// Actualiza los datos de un usuario existente (HU-12).
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserResponse>> UpdateUser(int id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updatedUser = await _userService.UpdateUserAsync(id, request, cancellationToken);
            if (updatedUser == null)
            {
                return NotFound(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"No se encontró el usuario con ID {id}.",
                    TraceId = HttpContext.TraceIdentifier
                });
            }

            return Ok(updatedUser);
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
    }

    /// <summary>
    /// Realiza la baja lógica de un usuario (HU-12).
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
    {
        var deleted = await _userService.DeleteUserAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new ErrorResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = $"No se encontró el usuario con ID {id}.",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return NoContent();
    }

    // =========================================================================
    // Helpers
    // =========================================================================

    private int? GetCurrentUserId()
    {
        // 1. Lectura de claims del token (cuando JWT este activo en HU-10/11)
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier) 
            ?? User.FindFirstValue("sub") 
            ?? User.FindFirstValue("id");

        if (int.TryParse(claimValue, out var id))
        {
            return id;
        }

        // 2. Fallback por header opcional para pruebas de integracion/desarrollo
        if (Request.Headers.TryGetValue("X-User-Id", out var headerValue) 
            && int.TryParse(headerValue, out var headerId))
        {
            return headerId;
        }

        return null;
    }
}
