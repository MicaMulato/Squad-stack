using DigitalArs.Application.DTOs.Reserves;
using DigitalArs.Application.Interfaces;
using DigitalArs.Domain.Entities;
using DigitalArs.Domain.Enums;
using Microsoft.EntityFrameworkCore;


namespace DigitalArs.Infrastructure.Services;

public class MoneyReserveService : IMoneyReserveService
{
    private readonly IUnitOfWork _unitOfWork;

    public MoneyReserveService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<MoneyReserveResponse>> GetReservesAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var accounts = await _unitOfWork.Repository<Account>().FindAsync(a => a.UserId == userId);
        var account = accounts.FirstOrDefault();
        if (account is null) return Array.Empty<MoneyReserveResponse>();

        var reserves = await _unitOfWork.Repository<MoneyReserve>()
            .Query()
            .Where(r => r.AccountId == account.Id && r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return reserves.Select(MapToResponse).ToList();
    }

    public async Task<MoneyReserveResponse> CreateReserveAsync(
        int userId,
        CreateReserveRequest request,
        CancellationToken cancellationToken = default)
    {
        var accountRepo = _unitOfWork.Repository<Account>();
        var accounts = await accountRepo.FindAsync(a => a.UserId == userId);
        var account = accounts.FirstOrDefault();

        if (account is null)
        {
            throw new KeyNotFoundException($"No se encontró la cuenta para el usuario {userId}.");
        }

        var reserve = new MoneyReserve
        {
            AccountId = account.Id,
            Name = request.Name.Trim(),
            TargetAmount = request.TargetAmount,
            CurrentBalance = 0m,
            Icon = string.IsNullOrWhiteSpace(request.Icon) ? "savings" : request.Icon.Trim(),
            Color = string.IsNullOrWhiteSpace(request.Color) ? "#0056D2" : request.Color.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _unitOfWork.Repository<MoneyReserve>().AddAsync(reserve);
        await _unitOfWork.CommitAsync();

        return MapToResponse(reserve);
    }

    public async Task<MoneyReserveResponse> DepositIntoReserveAsync(
        int userId,
        int reserveId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("El monto a asignar a la reserva debe ser mayor a cero.");
        }

        var accountRepo = _unitOfWork.Repository<Account>();
        var accounts = await accountRepo.FindAsync(a => a.UserId == userId);
        var account = accounts.FirstOrDefault();

        if (account is null)
        {
            throw new KeyNotFoundException("Cuenta no encontrada.");
        }

        var reserve = await _unitOfWork.Repository<MoneyReserve>().GetByIdAsync(reserveId);
        if (reserve is null || reserve.AccountId != account.Id || !reserve.IsActive)
        {
            throw new KeyNotFoundException("Reserva no encontrada.");
        }

        if (account.Money < amount)
        {
            throw new InvalidOperationException($"Saldo disponible insuficiente en cuenta. Disponible: ${account.Money:N2}, Requerido: ${amount:N2}.");
        }

        await _unitOfWork.BeginTransactionAsync();

        // Mover dinero de cuenta corriente hacia la reserva
        account.Money -= amount;
        reserve.CurrentBalance += amount;

        accountRepo.Update(account);
        _unitOfWork.Repository<MoneyReserve>().Update(reserve);

        // Registrar la transacción en el historial
        var txDeposit = new Transaction
        {
            AccountId = account.Id,
            Amount = amount,
            Type = TransactionType.TransferOut,
            Concept = $"Reserva: {reserve.Name}",
            Date = DateTime.UtcNow
        };
        await _unitOfWork.Repository<Transaction>().AddAsync(txDeposit);

        await _unitOfWork.CommitAsync();


        return MapToResponse(reserve);
    }

    public async Task<MoneyReserveResponse> WithdrawFromReserveAsync(
        int userId,
        int reserveId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("El monto a retirar de la reserva debe ser mayor a cero.");
        }

        var accountRepo = _unitOfWork.Repository<Account>();
        var accounts = await accountRepo.FindAsync(a => a.UserId == userId);
        var account = accounts.FirstOrDefault();

        if (account is null)
        {
            throw new KeyNotFoundException("Cuenta no encontrada.");
        }

        var reserve = await _unitOfWork.Repository<MoneyReserve>().GetByIdAsync(reserveId);
        if (reserve is null || reserve.AccountId != account.Id || !reserve.IsActive)
        {
            throw new KeyNotFoundException("Reserva no encontrada.");
        }

        if (reserve.CurrentBalance < amount)
        {
            throw new InvalidOperationException($"Saldo insuficiente en la reserva. Saldo actual: ${reserve.CurrentBalance:N2}, Requerido: ${amount:N2}.");
        }

        await _unitOfWork.BeginTransactionAsync();

        // Mover dinero de la reserva de vuelta a la cuenta corriente
        reserve.CurrentBalance -= amount;
        account.Money += amount;

        _unitOfWork.Repository<MoneyReserve>().Update(reserve);
        accountRepo.Update(account);

        // Registrar la transacción en el historial
        var txWithdraw = new Transaction
        {
            AccountId = account.Id,
            Amount = amount,
            Type = TransactionType.TransferIn,
            Concept = $"Reserva: {reserve.Name}",
            Date = DateTime.UtcNow
        };
        await _unitOfWork.Repository<Transaction>().AddAsync(txWithdraw);

        await _unitOfWork.CommitAsync();

        return MapToResponse(reserve);
    }

    public async Task DeleteReserveAsync(
        int userId,
        int reserveId,
        CancellationToken cancellationToken = default)
    {
        var accountRepo = _unitOfWork.Repository<Account>();
        var accounts = await accountRepo.FindAsync(a => a.UserId == userId);
        var account = accounts.FirstOrDefault();

        if (account is null)
        {
            throw new KeyNotFoundException("Cuenta no encontrada.");
        }

        var reserve = await _unitOfWork.Repository<MoneyReserve>().GetByIdAsync(reserveId);
        if (reserve is null || reserve.AccountId != account.Id || !reserve.IsActive)
        {
            throw new KeyNotFoundException("Reserva no encontrada.");
        }

        await _unitOfWork.BeginTransactionAsync();

        // Reintegrar todo el saldo restante a la cuenta principal
        if (reserve.CurrentBalance > 0)
        {
            account.Money += reserve.CurrentBalance;
            accountRepo.Update(account);
            reserve.CurrentBalance = 0m;
        }

        reserve.IsActive = false;
        _unitOfWork.Repository<MoneyReserve>().Update(reserve);

        await _unitOfWork.CommitAsync();
    }

    private static MoneyReserveResponse MapToResponse(MoneyReserve r)
    {
        double? progress = null;
        if (r.TargetAmount.HasValue && r.TargetAmount.Value > 0)
        {
            progress = Math.Min(100.0, Math.Round((double)(r.CurrentBalance / r.TargetAmount.Value * 100m), 1));
        }

        return new MoneyReserveResponse
        {
            Id = r.Id,
            Name = r.Name,
            TargetAmount = r.TargetAmount,
            CurrentBalance = r.CurrentBalance,
            Icon = r.Icon,
            Color = r.Color,
            CreatedAt = r.CreatedAt,
            ProgressPercentage = progress
        };
    }
}
