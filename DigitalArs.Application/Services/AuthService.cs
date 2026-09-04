using DigitalArs.Application.DTOs.Auth;
using DigitalArs.Application.Interfaces;
using DigitalArs.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DigitalArs.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        UserManager<User> userManager,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null) return null;
        //Validar contraseña
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid) return null;
        
        //Validar si el usuario esta activo
        if (user.IsDeleted)
        {
            throw new UnauthorizedAccessException("El usuario se encuentra inactivo");
        }
        //Obtener rol
        var roles = await _userManager.GetRolesAsync(user);
        var roleName = roles.FirstOrDefault() ?? user.Role?.Name ?? "User";

        // Token segun el rol
        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user, roleName);

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Role = roleName
        };
    }
}   