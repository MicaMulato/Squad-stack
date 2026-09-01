using DigitalArs.Application.DTOs.Common;
using DigitalArs.Application.DTOs.Users;
using DigitalArs.Application.Exceptions;
using DigitalArs.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalArs.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
// Espacio para agregar filtros HU-11
// [Authorize(Roles = "Admin")]

public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

// El manejo de excepciones se encuentra repetido hasta la construccion del middleware
// de GlobalExceptionHandler para un manejo mas limpio de errores. Una vez implementado


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
}
