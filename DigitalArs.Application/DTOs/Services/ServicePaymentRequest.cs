namespace DigitalArs.Application.DTOs.Services;

public record ServicePaymentRequest
{
    public int ServiceProviderId { get; init; }
    public string ReferenceNumber { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public int? ReserveId { get; init; }
}
