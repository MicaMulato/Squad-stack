using DigitalArs.Application.Interfaces;
using DigitalArs.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace DigitalArs.Infrastructure.Services;

public class PasswordHasherService : IPasswordHasher
{
    private readonly IPasswordHasher<User> _identityPasswordHasher;

    public PasswordHasherService(IPasswordHasher<User> identityPasswordHasher)
    {
        _identityPasswordHasher = identityPasswordHasher;
    }

    public string Hash(string password)
    {
        //PBKDF2 with HMAC-SHA256 
        return _identityPasswordHasher.HashPassword(null!, password);
    }

    public bool Verify(string password, string hashedPassword)
    {
        var result = _identityPasswordHasher.VerifyHashedPassword(null!, hashedPassword, password);
        return result != PasswordVerificationResult.Failed;
    }
}
