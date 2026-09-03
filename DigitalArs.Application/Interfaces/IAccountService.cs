using DigitalArs.Application.DTOs;
using DigitalArs.Application.DTOs.Accounts;

namespace DigitalArs.Application.Interfaces;

/// <summary>
/// Contrato para las operaciones sobre cuentas bancarias.
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Deposita el monto indicado en la cuenta del usuario autenticado (HU-15).
    /// </summary>
    /// <param name="userId">ID del usuario dueño de la cuenta.</param>
    /// <param name="amount">Monto a depositar. Debe ser mayor a 0 y no superar el límite configurado.</param>
    /// <returns>DTO con el nuevo saldo, el ID de la transacción y la fecha.</returns>
    Task<DepositResponseDto> DepositAsync(int userId, decimal amount);

    /// <summary>
    /// Consulta los datos y saldo de la cuenta del usuario (HU-14).
    /// </summary>
    Task<AccountResponse?> GetAccountByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta una cuenta por su ID (HU-14).
    /// </summary>
    Task<AccountResponse?> GetAccountByIdAsync(int id, CancellationToken cancellationToken = default);
}
