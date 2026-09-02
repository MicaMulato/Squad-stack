using DigitalArs.Application.Settings;
using DigitalArs.Application.DTOs;
using DigitalArs.Application.Interfaces;
using DigitalArs.Domain.Entities;
using DigitalArs.Domain.Enums;
using Microsoft.Extensions.Options;

namespace DigitalArs.Infrastructure.Services;

/// <summary>
/// Implementación de IAccountService.
/// Gestiona operaciones sobre cuentas usando el patrón Unit of Work
/// para garantizar atomicidad entre el saldo y el registro de la transacción.
/// </summary>
public class AccountService : IAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly DepositSettings _depositSettings;

    public AccountService(IUnitOfWork unitOfWork, IOptions<DepositSettings> depositOptions)
    {
        _unitOfWork = unitOfWork;
        _depositSettings = depositOptions.Value;
    }

    /// <inheritdoc />
    public async Task<DepositResponseDto> DepositAsync(int userId, decimal amount)
    {
        // ── Validación: límite máximo por operación (viene de appsettings.json) ──
        if (amount > _depositSettings.MaxAmountPerOperation)
        {
            throw new ArgumentException(
                $"El monto supera el límite máximo permitido de {_depositSettings.MaxAmountPerOperation:N2}.");
        }

        var accountRepo     = _unitOfWork.Repository<Account>();
        var transactionRepo = _unitOfWork.Repository<Transaction>();

        // ── Buscar la cuenta del usuario ──────────────────────────────────────
        var accounts = await accountRepo.FindAsync(a => a.UserId == userId);
        var account  = accounts.FirstOrDefault();

        if (account is null)
            throw new KeyNotFoundException($"No se encontró una cuenta para el usuario con ID {userId}.");

        if (account.IsBlocked)
            throw new InvalidOperationException("La cuenta está bloqueada y no puede recibir depósitos.");

        // ── Operación atómica: saldo + transaction en una sola transacción SQL ─
        await _unitOfWork.BeginTransactionAsync();

        account.Money += amount;
        accountRepo.Update(account);

        var transaction = new Transaction
        {
            AccountId = account.Id,
            Amount    = amount,
            Type      = TransactionType.Deposit,
            Concept   = "Depósito de fondos",
            Date      = DateTime.UtcNow
        };
        await transactionRepo.AddAsync(transaction);

        // CommitAsync llama SaveChanges + COMMIT.
        // Si algo falla ejecuta RollbackAsync automáticamente (ver UnitOfWork).
        await _unitOfWork.CommitAsync();

        return new DepositResponseDto
        {
            NewBalance     = account.Money,
            TransactionId  = transaction.Id,
            Date           = transaction.Date
        };
    }
}
