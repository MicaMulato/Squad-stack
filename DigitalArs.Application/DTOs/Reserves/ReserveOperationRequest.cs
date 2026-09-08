namespace DigitalArs.Application.DTOs.Reserves;

public record ReserveOperationRequest
{
    public decimal Amount { get; init; }
}
