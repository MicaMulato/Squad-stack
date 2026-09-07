namespace DigitalArs.Application.DTOs.FixedTermDeposits;

public class SimulateFixedTermDepositRequest
{
    public decimal Amount { get; set; }
    public int DurationDays { get; set; }
}

public class SimulateFixedTermDepositResponse
{
    public decimal Amount { get; set; }
    public decimal InterestRate { get; set; } = 19.0m;
    public int DurationDays { get; set; }
    public decimal InterestEarned { get; set; }
    public decimal FinalAmount { get; set; }
    public DateTime EstimatedClosingDate { get; set; }
}
