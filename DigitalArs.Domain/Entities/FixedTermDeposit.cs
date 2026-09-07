using DigitalArs.Domain.Enums;

namespace DigitalArs.Domain.Entities;

public class FixedTermDeposit : BaseEntity
{
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public decimal Amount { get; set; }
    public decimal InterestRate { get; set; } = 19.0m; // TNA 19%
    public int DurationDays { get; set; } // Mínimo 30 días

    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public DateTime ClosingDate { get; set; }

    public decimal InterestEarned { get; set; }
    public decimal FinalAmount { get; set; }

    public FixedTermDepositStatus Status { get; set; } = FixedTermDepositStatus.Active;
}
