using DigitalArs.Application.DTOs.FixedTermDeposits;

namespace DigitalArs.Application.Interfaces;

public interface IFixedTermDepositService
{
    Task<FixedTermDepositResponse> CreateAsync(int userId, CreateFixedTermDepositRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FixedTermDepositResponse>> GetMyDepositsAsync(int userId, CancellationToken cancellationToken = default);
    SimulateFixedTermDepositResponse Simulate(SimulateFixedTermDepositRequest request);
}
