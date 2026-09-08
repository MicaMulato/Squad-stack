using DigitalArs.Application.DTOs;
using DigitalArs.Application.DTOs.Accounts;
using DigitalArs.Application.Exceptions;
using DigitalArs.Application.Interfaces;
using DigitalArs.Application.Settings;
using DigitalArs.Domain.Entities;
using DigitalArs.Domain.Enums;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DigitalArs.Infrastructure.Services;

/// <summary>
/// Implementación de IAccountService.
/// Gestiona operaciones sobre cuentas usando el patrón Unit of Work.
/// </summary>
public class AccountService : IAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly DepositSettings _depositSettings;

    public AccountService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IOptions<DepositSettings> depositOptions)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _depositSettings = depositOptions.Value;
    }

    /// <inheritdoc />
    public async Task<AccountResponse?> GetAccountByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var accounts = await _unitOfWork.Repository<Account>()
            .FindAsync(a => a.UserId == userId);

        var account = accounts.FirstOrDefault();
        if (account == null)
        {
            return null;
        }

        return _mapper.Map<AccountResponse>(account);
    }

    /// <inheritdoc />
    public async Task<AccountResponse?> GetAccountByIdAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var account = await _unitOfWork.Repository<Account>().GetByIdAsync(accountId);
        if (account == null)
        {
            return null;
        }

        return _mapper.Map<AccountResponse>(account);
    }

    /// <inheritdoc />
    public async Task<AccountLookupResponse> LookupAccountAsync(int currentUserId, string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Debe ingresar un CVU o Alias para buscar al destinatario.");
        }

        var cleanQuery = query.Trim().ToLowerInvariant();

        var accountQuery = _unitOfWork.Repository<Account>()
            .Query()
            .Include(a => a.User);

        Account? destAccount = null;

        // 1. Si son 22 dígitos numéricos, buscar por CVU exacto
        if (cleanQuery.Length == 22 && cleanQuery.All(char.IsDigit))
        {
            destAccount = await accountQuery.FirstOrDefaultAsync(a => a.Cvu == cleanQuery, cancellationToken);
        }

        // 2. Si no encontró y es número entero corto, buscar por ID de cuenta
        if (destAccount == null && int.TryParse(cleanQuery, out var accountId))
        {
            destAccount = await accountQuery.FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
        }

        // 3. Buscar por Alias (insensible a mayúsculas)
        if (destAccount == null)
        {
            destAccount = await accountQuery.FirstOrDefaultAsync(a => a.Alias.ToLower() == cleanQuery, cancellationToken);
        }

        if (destAccount == null || destAccount.User == null || destAccount.User.IsDeleted)
        {
            throw new KeyNotFoundException("No encontramos ninguna cuenta registrada con ese CVU o Alias.");
        }

        // Validar autotransferencia
        if (destAccount.UserId == currentUserId)
        {
            throw new InvalidOperationException("No podés realizar una transferencia a tu propia cuenta.");
        }

        // Validar estado de la cuenta
        if (destAccount.IsBlocked)
        {
            throw new InvalidOperationException("La cuenta de destino se encuentra bloqueada y no puede recibir transferencias.");
        }

        // Enmascarar email para privacidad (ej: r***s@gmail.com)
        var email = destAccount.User.Email ?? "";
        var maskedEmail = email;
        if (email.Contains('@'))
        {
            var parts = email.Split('@');
            var userPart = parts[0];
            var domain = parts[1];
            if (userPart.Length > 2)
            {
                maskedEmail = $"{userPart[0]}***{userPart[^1]}@{domain}";
            }
        }

        var fullName = $"{destAccount.User.FirstName} {destAccount.User.LastName}".Trim();

        return new AccountLookupResponse
        {
            AccountId = destAccount.Id,
            Name = fullName,
            FirstName = destAccount.User.FirstName,
            LastName = destAccount.User.LastName,
            Cvu = destAccount.Cvu,
            Alias = destAccount.Alias,
            Bank = "DigitalArs Billetera Virtual",
            EmailMasked = maskedEmail,
            IsBlocked = destAccount.IsBlocked
        };
    }

    /// <inheritdoc />
    public async Task<AccountResponse> UpdateAliasAsync(int userId, string newAlias, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(newAlias))
        {
            throw new ArgumentException("El alias no puede estar vacío.");
        }

        var cleanAlias = newAlias.Trim().ToLowerInvariant();
        if (cleanAlias.Length < 4 || cleanAlias.Length > 50)
        {
            throw new ArgumentException("El alias debe tener entre 4 y 50 caracteres.");
        }

        var accountRepo = _unitOfWork.Repository<Account>();
        var accounts = await accountRepo.FindAsync(a => a.UserId == userId);
        var myAccount = accounts.FirstOrDefault();

        if (myAccount == null)
        {
            throw new KeyNotFoundException($"No se encontró una cuenta para el usuario con ID {userId}.");
        }

        if (string.Equals(myAccount.Alias, cleanAlias, StringComparison.OrdinalIgnoreCase))
        {
            return _mapper.Map<AccountResponse>(myAccount);
        }

        // Verificar unicidad en la base de datos
        var existingAccountWithAlias = await accountRepo.Query()
            .FirstOrDefaultAsync(a => a.Id != myAccount.Id && a.Alias.ToLower() == cleanAlias, cancellationToken);

        if (existingAccountWithAlias != null)
        {
            throw new ConflictException($"El alias '{cleanAlias}' ya se encuentra en uso por otra cuenta.");
        }

        myAccount.Alias = cleanAlias;
        accountRepo.Update(myAccount);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<AccountResponse>(myAccount);
    }

    /// <inheritdoc />
    public async Task<DepositResponseDto> DepositAsync(int userId, decimal amount, string? concept = null)
    {
        if (amount > _depositSettings.MaxAmountPerOperation)
        {
            throw new ArgumentException(
                $"El monto supera el límite máximo permitido de {_depositSettings.MaxAmountPerOperation:N2}.");
        }

        var accountRepo     = _unitOfWork.Repository<Account>();
        var transactionRepo = _unitOfWork.Repository<Transaction>();

        var accounts = await accountRepo.FindAsync(a => a.UserId == userId);
        var account  = accounts.FirstOrDefault();

        if (account is null)
            throw new KeyNotFoundException($"No se encontró una cuenta para el usuario con ID {userId}.");

        if (account.IsBlocked)
            throw new InvalidOperationException("La cuenta está bloqueada y no puede recibir depósitos.");

        await _unitOfWork.BeginTransactionAsync();

        account.Money += amount;
        accountRepo.Update(account);

        var motive = !string.IsNullOrWhiteSpace(concept) ? concept.Trim() : "Ahorro";

        var transaction = new Transaction
        {
            AccountId = account.Id,
            Amount    = amount,
            Type      = TransactionType.Deposit,
            Concept   = motive,
            Date      = DateTime.UtcNow
        };
        await transactionRepo.AddAsync(transaction);

        await _unitOfWork.CommitAsync();

        return new DepositResponseDto
        {
            NewBalance     = account.Money,
            TransactionId  = transaction.Id,
            Date           = transaction.Date
        };
    }
}
