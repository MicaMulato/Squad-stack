using DigitalArs.Domain.Enums;

namespace DigitalArs.Application.DTOs.FixedTermDeposits;

public class FixedTermDepositResponse
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public decimal InterestRate { get; set; } = 19.0m;
    public int DurationDays { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime ClosingDate { get; set; }
    public decimal InterestEarned { get; set; }
    public decimal FinalAmount { get; set; }
    public FixedTermDepositStatus Status { get; set; }
    public string StatusName => Status.ToString();
}
