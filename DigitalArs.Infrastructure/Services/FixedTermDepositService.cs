using DigitalArs.Application.DTOs.FixedTermDeposits;
using DigitalArs.Application.Exceptions;
using DigitalArs.Application.Interfaces;
using DigitalArs.Domain.Entities;
using DigitalArs.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using DigitalArs.Infrastructure.Data;

namespace DigitalArs.Infrastructure.Services;

public class FixedTermDepositService : IFixedTermDepositService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private const decimal AnnualInterestRate = 19.0m; // 19% TNA

    public FixedTermDepositService(ApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<FixedTermDepositResponse> CreateAsync(int userId, CreateFixedTermDepositRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Obtener la cuenta activa del usuario
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);

        if (account == null)
            throw new NotFoundException("No se encontró una cuenta bancaria asociada a este usuario.");

        if (account.IsBlocked)
            throw new BadRequestException("La cuenta se encuentra bloqueada para realizar inversiones.");

        // 2. Validar saldo suficiente
        if (account.Money < request.Amount)
            throw new BadRequestException($"Saldo insuficiente. Tu saldo actual es de {account.Money:C} y el monto a invertir es de {request.Amount:C}.");

        // 3. Cálculos financieros (19% TNA)
        var now = DateTime.UtcNow;
        var closingDate = now.AddDays(request.DurationDays);
        var interestEarned = Math.Round(request.Amount * (AnnualInterestRate / 100m / 365m) * request.DurationDays, 2);
        var finalAmount = request.Amount + interestEarned;

        // 4. Débito de saldo
        account.Money -= request.Amount;

        // 5. Creación del Plazo Fijo
        var deposit = new FixedTermDeposit
        {
            AccountId = account.Id,
            Amount = request.Amount,
            InterestRate = AnnualInterestRate,
            DurationDays = request.DurationDays,
            CreationDate = now,
            ClosingDate = closingDate,
            InterestEarned = interestEarned,
            FinalAmount = finalAmount,
            Status = FixedTermDepositStatus.Active
        };

        await _context.FixedTermDeposits.AddAsync(deposit, cancellationToken);

        // 6. Registro de movimiento en transacciones
        var transaction = new Transaction
        {
            AccountId = account.Id,
            Amount = request.Amount,
            Type = TransactionType.FixedDeposit,
            Concept = $"Constitución de Plazo Fijo ({request.DurationDays} días @ {AnnualInterestRate}% TNA)",
            Date = now
        };

        await _context.Transactions.AddAsync(transaction, cancellationToken);

        // 7. Persistencia atómica
        await _unitOfWork.SaveChangesAsync();

        return new FixedTermDepositResponse
        {
            Id = deposit.Id,
            AccountId = deposit.AccountId,
            Amount = deposit.Amount,
            InterestRate = deposit.InterestRate,
            DurationDays = deposit.DurationDays,
            CreationDate = deposit.CreationDate,
            ClosingDate = deposit.ClosingDate,
            InterestEarned = deposit.InterestEarned,
            FinalAmount = deposit.FinalAmount,
            Status = deposit.Status
        };
    }

    public async Task<IReadOnlyList<FixedTermDepositResponse>> GetMyDepositsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);

        if (account == null)
            return Array.Empty<FixedTermDepositResponse>();

        var deposits = await _context.FixedTermDeposits
            .AsNoTracking()
            .Where(f => f.AccountId == account.Id)
            .OrderByDescending(f => f.CreationDate)
            .ToListAsync(cancellationToken);

        return deposits.Select(d => new FixedTermDepositResponse
        {
            Id = d.Id,
            AccountId = d.AccountId,
            Amount = d.Amount,
            InterestRate = d.InterestRate,
            DurationDays = d.DurationDays,
            CreationDate = d.CreationDate,
            ClosingDate = d.ClosingDate,
            InterestEarned = d.InterestEarned,
            FinalAmount = d.FinalAmount,
            Status = d.Status
        }).ToList();
    }

    public SimulateFixedTermDepositResponse Simulate(SimulateFixedTermDepositRequest request)
    {
        var interestEarned = Math.Round(request.Amount * (AnnualInterestRate / 100m / 365m) * request.DurationDays, 2);
        var finalAmount = request.Amount + interestEarned;
        var estimatedClosingDate = DateTime.UtcNow.AddDays(request.DurationDays);

        return new SimulateFixedTermDepositResponse
        {
            Amount = request.Amount,
            InterestRate = AnnualInterestRate,
            DurationDays = request.DurationDays,
            InterestEarned = interestEarned,
            FinalAmount = finalAmount,
            EstimatedClosingDate = estimatedClosingDate
        };
    }
}
