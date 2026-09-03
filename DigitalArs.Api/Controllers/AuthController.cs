using DigitalArs.Application.DTOs.Auth;
using DigitalArs.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DigitalArs.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    //POST api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var resultado = await _authService.LoginAsync(request);
            if (resultado is null)
            {
                //Error 401
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
            }

            //Login exitoso
            return Ok(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            //Usuario inactivo
            return Unauthorized(new { message = ex.Message });
        }
    }
}