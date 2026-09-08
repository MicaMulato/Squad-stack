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
    Task<DepositResponseDto> DepositAsync(int userId, decimal amount, string? concept = null);

    /// <summary>
    /// Consulta los datos y saldo de la cuenta del usuario (HU-14).
    /// </summary>
    Task<AccountResponse?> GetAccountByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta una cuenta por su ID (HU-14).
    /// </summary>
    Task<AccountResponse?> GetAccountByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca y resuelve los datos públicos de una cuenta destinataria mediante CVU, Alias o ID.
    /// </summary>
    Task<AccountLookupResponse> LookupAccountAsync(int currentUserId, string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Modifica el alias bancario de la cuenta del usuario autenticado, verificando unicidad.
    /// </summary>
    Task<AccountResponse> UpdateAliasAsync(int userId, string newAlias, CancellationToken cancellationToken = default);
}
