using DigitalArs.Application.DTOs.Accounts;


namespace DigitalArs.Application.Interfaces;

public interface IAccountService
{
    // Consultar datos y saldo de la cuenta propia
    Task<AccountResponse?> GetAccountByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<AccountResponse?> GetAccountByIdAsync(int id, CancellationToken cancellationToken = default);
}
