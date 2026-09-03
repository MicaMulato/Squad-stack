using DigitalArs.Domain.Entities;

namespace DigitalArs.Application.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user, string role);
}