using DigitalArs.Application.DTOs;
using DigitalArs.Application.Interfaces;
using DigitalArs.Domain.Entities;
using DigitalArs.Domain.Enums;

namespace DigitalArs.Infrastructure.Services;

/// <summary>
/// Implementación de ITransactionService.
/// Gestiona transferencias entre cuentas usando Unit of Work para garantizar
/// que ambos registros (TransferOut + TransferIn) y ambos saldos se actualicen
/// de forma atómica o no se modifique nada.
/// </summary>
public class TransactionService : ITransactionService
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<TransferResponseDto> TransferAsync(
        int sourceUserId,
        int destinationAccountId,
        decimal amount)
    {
        var accountRepo     = _unitOfWork.Repository<Account>();
        var transactionRepo = _unitOfWork.Repository<Transaction>();

        // ── 1. Buscar cuenta origen (del usuario autenticado) ─────────────────
        var sourceAccounts = await accountRepo.FindAsync(a => a.UserId == sourceUserId);
        var sourceAccount  = sourceAccounts.FirstOrDefault();

        if (sourceAccount is null)
            throw new KeyNotFoundException(
                $"No se encontró una cuenta para el usuario con ID {sourceUserId}.");

        // ── 2. Buscar cuenta destino por su ID de cuenta ──────────────────────
        var destAccount = await accountRepo.GetByIdAsync(destinationAccountId);

        if (destAccount is null)
            throw new KeyNotFoundException(
                $"La cuenta destino con ID {destinationAccountId} no existe.");

        if (destAccount.IsBlocked)
            throw new InvalidOperationException(
                $"La cuenta destino con ID {destinationAccountId} está bloqueada y no puede recibir transferencias.");

        // ── 3. Validar que no sea autotransferencia ───────────────────────────
        if (sourceAccount.Id == destinationAccountId)
            throw new ArgumentException(
                "No se puede transferir dinero a tu propia cuenta.");

        // ── 4. Validar saldo suficiente ───────────────────────────────────────
        if (sourceAccount.Money < amount)
            throw new InvalidOperationException(
                $"Saldo insuficiente. Disponible: {sourceAccount.Money:N2}, requerido: {amount:N2}.");

        // ── 5. Operación atómica ──────────────────────────────────────────────
        await _unitOfWork.BeginTransactionAsync();

        var transferDate = DateTime.UtcNow;

        // Actualizar saldos
        sourceAccount.Money -= amount;
        destAccount.Money   += amount;
        accountRepo.Update(sourceAccount);
        accountRepo.Update(destAccount);

        // Registro desde la perspectiva de quien envía (pierde dinero)
        var transferOut = new Transaction
        {
            AccountId   = sourceAccount.Id,
            ToAccountId = destAccount.Id,
            Amount      = amount,
            Type        = TransactionType.TransferOut,
            Concept     = $"Transferencia enviada a cuenta #{destAccount.Id}",
            Date        = transferDate
        };

        // Registro desde la perspectiva de quien recibe (gana dinero)
        var transferIn = new Transaction
        {
            AccountId   = destAccount.Id,
            ToAccountId = sourceAccount.Id,
            Amount      = amount,
            Type        = TransactionType.TransferIn,
            Concept     = $"Transferencia recibida de cuenta #{sourceAccount.Id}",
            Date        = transferDate
        };

        await transactionRepo.AddAsync(transferOut);
        await transactionRepo.AddAsync(transferIn);

        // SaveChanges + COMMIT. Si algo falla → RollbackAsync automático (UnitOfWork).
        await _unitOfWork.CommitAsync();

        return new TransferResponseDto
        {
            TransferOutId = transferOut.Id,
            TransferInId  = transferIn.Id,
            Amount        = amount,
            NewBalance    = sourceAccount.Money,
            Date          = transferDate
        };
    }
}
