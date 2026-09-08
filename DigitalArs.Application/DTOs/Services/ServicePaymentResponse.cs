using DigitalArs.Domain.Enums;

namespace DigitalArs.Application.DTOs.Services;

public record ServicePaymentResponse
{
    public int Id { get; init; }
    public string ProviderName { get; init; } = string.Empty;
    public ServiceCategory Category { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string ReferenceNumber { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime PaymentDate { get; init; }
    public string ReceiptNumber { get; init; } = string.Empty;
    public decimal RemainingAccountBalance { get; init; }
    public decimal? RemainingReserveBalance { get; init; }
    public string? ReserveName { get; init; }
}
