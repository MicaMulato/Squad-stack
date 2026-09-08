using DigitalArs.Application.DTOs.Services;
using DigitalArs.Application.Interfaces;
using DigitalArs.Domain.Entities;
using DigitalArs.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DigitalArs.Infrastructure.Services;

public class ServicePaymentService : IServicePaymentService
{
    private readonly IUnitOfWork _unitOfWork;

    public ServicePaymentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ServiceProviderDto>> GetProvidersAsync(
        ServiceCategory? category = null,
        CancellationToken cancellationToken = default)
    {
        var providerRepo = _unitOfWork.Repository<ServiceProvider>();
        var query = providerRepo.Query().Where(p => p.IsActive);

        if (category.HasValue)
        {
            query = query.Where(p => p.Category == category.Value);
        }

        var providers = await query
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return providers.Select(p => new ServiceProviderDto
        {
            Id = p.Id,
            Name = p.Name,
            Category = p.Category,
            CategoryName = GetCategoryDisplayName(p.Category),
            CodeLabel = p.CodeLabel,
            IconName = p.IconName
        }).ToList();
    }

    public async Task<SimulateInvoiceResponse> SimulateInvoiceAsync(
        int providerId,
        string referenceNumber,
        CancellationToken cancellationToken = default)
    {
        var provider = await _unitOfWork.Repository<ServiceProvider>().GetByIdAsync(providerId);
        if (provider is null || !provider.IsActive)
        {
            throw new KeyNotFoundException("El proveedor de servicios no existe o se encuentra inactivo.");
        }

        // Generación determinista / coherente de simulación según el código de referencia
        var hash = Math.Abs(referenceNumber.GetHashCode());
        var simulatedAmount = 4500m + (hash % 35000) + ((hash % 100) / 100m);
        var dueDate = DateTime.UtcNow.Date.AddDays(7 + (hash % 14));
        var invoiceNumber = $"FACT-{provider.Name.Substring(0, Math.Min(4, provider.Name.Length)).ToUpper()}-{hash % 90000 + 10000}";

        return new SimulateInvoiceResponse
        {
            ServiceProviderId = provider.Id,
            ProviderName = provider.Name,
            ReferenceNumber = referenceNumber,
            ClientName = "Titular de Servicios",
            Amount = Math.Round(simulatedAmount, 2),
            DueDate = dueDate,
            InvoiceNumber = invoiceNumber
        };
    }

    public async Task<ServicePaymentResponse> PayServiceAsync(
        int userId,
        ServicePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var accountRepo = _unitOfWork.Repository<Account>();
        var accounts = await accountRepo.FindAsync(a => a.UserId == userId);
        var account = accounts.FirstOrDefault();

        if (account is null)
        {
            throw new KeyNotFoundException($"No se encontró una cuenta para el usuario {userId}.");
        }

        if (account.IsBlocked)
        {
            throw new InvalidOperationException("La cuenta se encuentra bloqueada para operar.");
        }

        var provider = await _unitOfWork.Repository<ServiceProvider>().GetByIdAsync(request.ServiceProviderId);
        if (provider is null || !provider.IsActive)
        {
            throw new KeyNotFoundException("El proveedor de servicios seleccionado no existe o no está activo.");
        }

        await _unitOfWork.BeginTransactionAsync();

        MoneyReserve? reserve = null;
        if (request.ReserveId.HasValue)
        {
            reserve = await _unitOfWork.Repository<MoneyReserve>().GetByIdAsync(request.ReserveId.Value);
            if (reserve is null || reserve.AccountId != account.Id || !reserve.IsActive)
            {
                throw new KeyNotFoundException("La reserva seleccionada no existe o no pertenece a tu cuenta.");
            }

            if (reserve.CurrentBalance < request.Amount)
            {
                throw new InvalidOperationException(
                    $"Saldo insuficiente en la reserva '{reserve.Name}'. Disponible: ${reserve.CurrentBalance:N2}, Requerido: ${request.Amount:N2}.");
            }

            reserve.CurrentBalance -= request.Amount;
            _unitOfWork.Repository<MoneyReserve>().Update(reserve);
        }
        else
        {
            if (account.Money < request.Amount)
            {
                throw new InvalidOperationException(
                    $"Saldo insuficiente en cuenta. Disponible: ${account.Money:N2}, Requerido: ${request.Amount:N2}.");
            }

            account.Money -= request.Amount;
            accountRepo.Update(account);
        }

        var categoryDisplayName = GetCategoryDisplayName(provider.Category);
        var concept = $"Servicio: {categoryDisplayName}";


        var paymentDate = DateTime.UtcNow;
        var transaction = new Transaction
        {
            AccountId = account.Id,
            Amount = request.Amount,
            Type = TransactionType.Payment,
            Concept = concept,
            Date = paymentDate
        };
        await _unitOfWork.Repository<Transaction>().AddAsync(transaction);
        await _unitOfWork.CommitAsync();

        var receiptNumber = $"REC-SERV-{paymentDate:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
        var payment = new ServicePayment
        {
            AccountId = account.Id,
            ServiceProviderId = provider.Id,
            ReferenceNumber = request.ReferenceNumber.Trim(),
            Amount = request.Amount,
            PaymentDate = paymentDate,
            ReceiptNumber = receiptNumber,
            TransactionId = transaction.Id,
            ReserveId = reserve?.Id
        };
        await _unitOfWork.Repository<ServicePayment>().AddAsync(payment);
        await _unitOfWork.CommitAsync();

        return new ServicePaymentResponse
        {
            Id = payment.Id,
            ProviderName = provider.Name,
            Category = provider.Category,
            CategoryName = GetCategoryDisplayName(provider.Category),
            ReferenceNumber = payment.ReferenceNumber,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            ReceiptNumber = payment.ReceiptNumber,
            RemainingAccountBalance = account.Money,
            RemainingReserveBalance = reserve?.CurrentBalance,
            ReserveName = reserve?.Name
        };
    }

    public async Task<IReadOnlyList<ServicePaymentResponse>> GetMyPaymentsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var accounts = await _unitOfWork.Repository<Account>().FindAsync(a => a.UserId == userId);
        var account = accounts.FirstOrDefault();
        if (account is null) return Array.Empty<ServicePaymentResponse>();

        var payments = await _unitOfWork.Repository<ServicePayment>()
            .Query()
            .Include(sp => sp.ServiceProvider)
            .Include(sp => sp.Reserve)
            .Where(sp => sp.AccountId == account.Id)
            .OrderByDescending(sp => sp.PaymentDate)
            .ToListAsync(cancellationToken);

        return payments.Select(sp => new ServicePaymentResponse
        {
            Id = sp.Id,
            ProviderName = sp.ServiceProvider.Name,
            Category = sp.ServiceProvider.Category,
            CategoryName = GetCategoryDisplayName(sp.ServiceProvider.Category),
            ReferenceNumber = sp.ReferenceNumber,
            Amount = sp.Amount,
            PaymentDate = sp.PaymentDate,
            ReceiptNumber = sp.ReceiptNumber,
            RemainingAccountBalance = account.Money,
            RemainingReserveBalance = sp.Reserve?.CurrentBalance,
            ReserveName = sp.Reserve?.Name
        }).ToList();
    }

    private static string GetCategoryDisplayName(ServiceCategory category) => category switch
    {
        ServiceCategory.Electricity => "Electricidad (Luz)",
        ServiceCategory.Water => "Agua",
        ServiceCategory.Gas => "Gas Natural",
        ServiceCategory.TelephonyAndInternet => "Telefonía e Internet",
        ServiceCategory.Taxes => "Impuestos y Tasas",
        _ => "Otros Servicios"
    };

}
