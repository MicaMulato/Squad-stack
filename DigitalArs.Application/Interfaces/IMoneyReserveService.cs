using DigitalArs.Application.DTOs.Reserves;

namespace DigitalArs.Application.Interfaces;

public interface IMoneyReserveService
{
    Task<IReadOnlyList<MoneyReserveResponse>> GetReservesAsync(int userId, CancellationToken cancellationToken = default);
    Task<MoneyReserveResponse> CreateReserveAsync(int userId, CreateReserveRequest request, CancellationToken cancellationToken = default);
    Task<MoneyReserveResponse> DepositIntoReserveAsync(int userId, int reserveId, decimal amount, CancellationToken cancellationToken = default);
    Task<MoneyReserveResponse> WithdrawFromReserveAsync(int userId, int reserveId, decimal amount, CancellationToken cancellationToken = default);
    Task DeleteReserveAsync(int userId, int reserveId, CancellationToken cancellationToken = default);
}
