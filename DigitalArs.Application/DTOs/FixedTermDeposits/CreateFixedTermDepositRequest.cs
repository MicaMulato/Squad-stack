namespace DigitalArs.Application.DTOs.FixedTermDeposits;

public class CreateFixedTermDepositRequest
{
    public decimal Amount { get; set; }
    public int DurationDays { get; set; }
}
