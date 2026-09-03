using DigitalArs.Application.DTOs.Auth;
using DigitalArs.Application.Interfaces;
using DigitalArs.Domain.Entities;

namespace DigitalArs.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var users = await _unitOfWork.Users.FindAsync(u => u.Email == request.Email);
        var user = users.FirstOrDefault();
        //Chequear que exista el usuario y que la contraseña sea correcta
        if (user is null || string.IsNullOrEmpty(user.PasswordHash)) return null;

        //Verificar la contraseña usando el IPasswordHasher
        if (!_passwordHasher.Verify(request.Password, user.PasswordHash)) return null;

        //Validar si el usuario esta activo
        if (user.IsDeleted)
        {
            throw new UnauthorizedAccessException("El usuario se encuentra inactivo");
        }

        var role = user.Role?.Name ?? "User";

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user, role);

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt
        };
    }
}   