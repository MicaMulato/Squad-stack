using DigitalArs.Application.DTOs.Auth;

namespace DigitalArs.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}

