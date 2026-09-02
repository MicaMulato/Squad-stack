using DigitalArs.Application.DTOs;

namespace DigitalArs.Application.Interfaces;

/// <summary>
/// Contrato para las operaciones sobre cuentas bancarias.
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Deposita el monto indicado en la cuenta del usuario autenticado.
    /// </summary>
    /// <param name="userId">ID del usuario dueño de la cuenta.</param>
    /// <param name="amount">Monto a depositar. Debe ser mayor a 0 y no superar el límite configurado.</param>
    /// <returns>DTO con el nuevo saldo, el ID de la transacción y la fecha.</returns>
    Task<DepositResponseDto> DepositAsync(int userId, decimal amount);
}
