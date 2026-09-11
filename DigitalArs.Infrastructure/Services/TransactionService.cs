using DigitalArs.Application.DTOs;
using DigitalArs.Application.Interfaces;
using DigitalArs.Domain.Entities;
using DigitalArs.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DigitalArs.Infrastructure.Services;

/// <summary>
/// Implementación de ITransactionService.
/// Gestiona transferencias entre cuentas y el historial de movimientos.
/// </summary>
public class TransactionService : ITransactionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public TransactionService(
        IUnitOfWork unitOfWork,
        INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    /// <inheritdoc />
    public async Task<TransferResponseDto> TransferAsync(
        int sourceUserId,
        int destinationAccountId,
        decimal amount,
        string? concept = null,
        int? reserveId = null)
    {
        var accountRepo     = _unitOfWork.Repository<Account>();
        var transactionRepo = _unitOfWork.Repository<Transaction>();

        // ── 1. Buscar cuenta origen ───────────────────────────────────────────
        var sourceAccount = await accountRepo.Query()
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.UserId == sourceUserId);

        if (sourceAccount is null)
            throw new KeyNotFoundException(
                $"No se encontró una cuenta para el usuario con ID {sourceUserId}.");

        // ── 2. Buscar cuenta destino ──────────────────────────────────────────
        var destAccount = await accountRepo.GetByIdAsync(destinationAccountId);

        if (destAccount is null)
            throw new KeyNotFoundException(
                $"La cuenta destino con ID {destinationAccountId} no existe.");

        if (destAccount.IsBlocked)
            throw new InvalidOperationException(
                $"La cuenta destino con ID {destinationAccountId} está bloqueada y no puede recibir transferencias.");

        // ── 3. Validar autotransferencia ──────────────────────────────────────
        if (sourceAccount.Id == destinationAccountId)
            throw new ArgumentException(
                "No se puede transferir dinero a tu propia cuenta.");

        // ── 4. Validar saldo suficiente (Cuenta o Reserva) ─────────────────────
        MoneyReserve? reserve = null;
        if (reserveId.HasValue)
        {
            reserve = await _unitOfWork.Repository<MoneyReserve>().GetByIdAsync(reserveId.Value);
            if (reserve is null || reserve.AccountId != sourceAccount.Id || !reserve.IsActive)
            {
                throw new KeyNotFoundException("La reserva seleccionada no existe o no pertenece a tu cuenta.");
            }

            if (reserve.CurrentBalance < amount)
            {
                throw new InvalidOperationException(
                    $"Saldo insuficiente en la reserva '{reserve.Name}'. Disponible: ${reserve.CurrentBalance:N2}, requerido: ${amount:N2}.");
            }
        }
        else
        {
            if (sourceAccount.Money < amount)
                throw new InvalidOperationException(
                    $"Saldo insuficiente. Disponible: {sourceAccount.Money:N2}, requerido: {amount:N2}.");
        }

        // ── 5. Operación atómica ──────────────────────────────────────────────
        await _unitOfWork.BeginTransactionAsync();

        var transferDate = DateTime.UtcNow;

        if (reserve != null)
        {
            reserve.CurrentBalance -= amount;
            _unitOfWork.Repository<MoneyReserve>().Update(reserve);
        }
        else
        {
            sourceAccount.Money -= amount;
            accountRepo.Update(sourceAccount);
        }

        destAccount.Money   += amount;
        accountRepo.Update(destAccount);

        var motive = !string.IsNullOrWhiteSpace(concept) ? concept.Trim() : null;
        var reserveSuffix = reserve != null ? $" (desde reserva '{reserve.Name}')" : string.Empty;
        var outConcept = motive != null
            ? $"{motive}{reserveSuffix}"
            : $"Transferencia enviada a cuenta #{destAccount.Id}{reserveSuffix}";
        var inConcept = motive != null ? $"Transferencia recibida · {motive}" : $"Transferencia recibida de cuenta #{sourceAccount.Id}";

        var transferOut = new Transaction
        {
            AccountId   = sourceAccount.Id,
            ToAccountId = destAccount.Id,
            Amount      = amount,
            Type        = TransactionType.TransferOut,
            Concept     = outConcept,
            Date        = transferDate
        };

        var transferIn = new Transaction
        {
            AccountId   = destAccount.Id,
            ToAccountId = sourceAccount.Id,
            Amount      = amount,
            Type        = TransactionType.TransferIn,
            Concept     = inConcept,
            Date        = transferDate
        };


        await transactionRepo.AddAsync(transferOut);
        await transactionRepo.AddAsync(transferIn);

        await _unitOfWork.CommitAsync();

        // Notificación al receptor de la transferencia (quien recibe dinero)
        try
        {
            var senderName = !string.IsNullOrWhiteSpace(sourceAccount.User?.FirstName)
                ? $"{sourceAccount.User.FirstName} {sourceAccount.User.LastName}".Trim()
                : (!string.IsNullOrWhiteSpace(sourceAccount.User?.Email) ? sourceAccount.User.Email : $"la cuenta #{sourceAccount.Id}");

            var motiveText = !string.IsNullOrWhiteSpace(motive) ? motive : "Varios";

            await _notificationService.CreateNotificationAsync(
                destAccount.UserId,
                "Recibiste una transferencia",
                $"Recibiste una transferencia de ${amount:N2} de {senderName}. Motivo: {motiveText}.",
                "TransferIn",
                "/history"
            );

        }
        catch
        {
            // Failsafe para no romper la respuesta de transferencia
        }

        return new TransferResponseDto
        {
            TransferOutId = transferOut.Id,
            TransferInId  = transferIn.Id,
            Amount        = amount,
            NewBalance    = sourceAccount.Money,
            Date          = transferDate
        };
    }

    /// <inheritdoc />
    public async Task<PagedResultDto<TransactionItemDto>> GetHistoryAsync(
        int userId,
        TransactionQueryDto queryDto)
    {
        var accountRepo     = _unitOfWork.Repository<Account>();
        var transactionRepo = _unitOfWork.Repository<Transaction>();

        // ── 1. Verificar que el usuario tiene cuenta ──────────────────────────
        var accounts = await accountRepo.FindAsync(a => a.UserId == userId);
        var account  = accounts.FirstOrDefault();

        if (account is null)
            throw new KeyNotFoundException(
                $"No se encontró una cuenta para el usuario con ID {userId}.");

        // ── 2. Construir la query sin ejecutarla aún (IQueryable) ─────────────
        // Punto de partida: todas las transacciones de la cuenta del usuario
        var query = transactionRepo.Query()
            .Where(t => t.AccountId == account.Id);

        // Aplicar filtros opcionales encadenados.
        // Filtro por agrupación de movimientos (ingresos / egresos)
        if (!string.IsNullOrWhiteSpace(queryDto.MovementType))
        {
            var mt = queryDto.MovementType.Trim().ToLowerInvariant();
            if (mt == "income" || mt == "ingreso" || mt == "ingresos")
            {
                query = query.Where(t => t.Type == TransactionType.Deposit || t.Type == TransactionType.TransferIn);
            }
            else if (mt == "expense" || mt == "egreso" || mt == "egresos")
            {
                query = query.Where(t => t.Type == TransactionType.TransferOut
                                      || t.Type == TransactionType.FixedDeposit
                                      || t.Type == TransactionType.Payment);
            }
        }
        else if (queryDto.Type.HasValue)
        {
            query = query.Where(t => t.Type == queryDto.Type.Value);
        }

        // Filtro por búsqueda textual en el concepto
        if (!string.IsNullOrWhiteSpace(queryDto.Search))
        {
            var s = queryDto.Search.Trim();
            var lowerS = s.ToLowerInvariant();
            var isInvestment = lowerS.Contains("invers") || lowerS.Contains("plazo fijo") || lowerS.Contains("rendimiento");
            var isService = lowerS.Contains("servicio") || lowerS.Contains("pago");
            var isDeposit = lowerS.Contains("deposito") || lowerS.Contains("depósito");

            query = query.Where(t =>
                (t.Concept != null && EF.Functions.Like(t.Concept, $"%{s}%"))
                || (isInvestment && t.Type == TransactionType.FixedDeposit)
                || (isService && t.Type == TransactionType.Payment)
                || (isDeposit && t.Type == TransactionType.Deposit)
            );
        }

        if (queryDto.DateFrom.HasValue)
        {
            var minDate = queryDto.DateFrom.Value.TimeOfDay == TimeSpan.Zero
                ? queryDto.DateFrom.Value.Date.AddHours(-14)
                : queryDto.DateFrom.Value;
            query = query.Where(t => t.Date >= minDate);
        }


        if (queryDto.DateTo.HasValue)
        {
            var maxDate = queryDto.DateTo.Value.TimeOfDay == TimeSpan.Zero
                ? queryDto.DateTo.Value.Date.AddDays(1).AddHours(14)
                : queryDto.DateTo.Value;
            query = query.Where(t => t.Date <= maxDate);
        }

        if (queryDto.AmountMin.HasValue)
            query = query.Where(t => t.Amount >= queryDto.AmountMin.Value);

        if (queryDto.AmountMax.HasValue)
            query = query.Where(t => t.Amount <= queryDto.AmountMax.Value);

        // Orden descendente por fecha (movimientos más recientes primero)
        query = query.OrderByDescending(t => t.Date);

        // ── 3. Contar el total ANTES de paginar (1 query SQL: SELECT COUNT(*)) ─
        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)queryDto.PageSize);

        // ── 4. Proyección + paginación (1 query SQL: SELECT ... OFFSET ... FETCH)
        // .Select() proyecta directamente al DTO: EF trae solo las columnas
        // necesarias y no carga las propiedades de navegación (Account, User).
        // Esto evita el problema N+1.
        var items = await query
            .Skip((queryDto.Page - 1) * queryDto.PageSize)
            .Take(queryDto.PageSize)
            .Select(t => new TransactionItemDto
            {
                Id          = t.Id,
                Amount      = t.Amount,
                Type        = t.Type,
                Concept     = t.Concept,
                Date        = t.Date,
                ToAccountId = t.ToAccountId
            })
            .ToListAsync();

        return new PagedResultDto<TransactionItemDto>
        {
            Items      = items,
            Page       = queryDto.Page,
            PageSize   = queryDto.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }
}
