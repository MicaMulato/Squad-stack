using DigitalArs.Application.DTOs.Services;
using DigitalArs.Domain.Enums;

namespace DigitalArs.Application.Interfaces;

public interface IServicePaymentService
{
    Task<IReadOnlyList<ServiceProviderDto>> GetProvidersAsync(ServiceCategory? category = null, CancellationToken cancellationToken = default);
    Task<SimulateInvoiceResponse> SimulateInvoiceAsync(int providerId, string referenceNumber, CancellationToken cancellationToken = default);
    Task<ServicePaymentResponse> PayServiceAsync(int userId, ServicePaymentRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServicePaymentResponse>> GetMyPaymentsAsync(int userId, CancellationToken cancellationToken = default);
}
