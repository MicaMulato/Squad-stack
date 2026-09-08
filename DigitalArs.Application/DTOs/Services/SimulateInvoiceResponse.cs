namespace DigitalArs.Application.DTOs.Services;

public record SimulateInvoiceResponse
{
    public int ServiceProviderId { get; init; }
    public string ProviderName { get; init; } = string.Empty;
    public string ReferenceNumber { get; init; } = string.Empty;
    public string ClientName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime DueDate { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
}
