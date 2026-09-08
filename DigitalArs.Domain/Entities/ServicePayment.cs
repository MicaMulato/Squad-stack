namespace DigitalArs.Domain.Entities;

public class ServicePayment : BaseEntity
{
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public int ServiceProviderId { get; set; }
    public ServiceProvider ServiceProvider { get; set; } = null!;

    public string ReferenceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string ReceiptNumber { get; set; } = string.Empty;

    public int TransactionId { get; set; }
    public Transaction Transaction { get; set; } = null!;

    public int? ReserveId { get; set; }
    public MoneyReserve? Reserve { get; set; }
}
