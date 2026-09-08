namespace DigitalArs.Domain.Entities;

public class MoneyReserve : BaseEntity
{
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public decimal? TargetAmount { get; set; }
    public decimal CurrentBalance { get; set; } = 0m;
    public string? Icon { get; set; }
    public string? Color { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public ICollection<ServicePayment> ServicePayments { get; set; } = new List<ServicePayment>();
}
