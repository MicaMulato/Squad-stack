namespace DigitalArs.Application.DTOs.Reserves;

public record CreateReserveRequest
{
    public string Name { get; init; } = string.Empty;
    public decimal? TargetAmount { get; init; }
    public string? Icon { get; init; }
    public string? Color { get; init; }
}
