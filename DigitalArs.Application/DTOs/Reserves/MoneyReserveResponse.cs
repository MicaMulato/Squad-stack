namespace DigitalArs.Application.DTOs.Reserves;

public record MoneyReserveResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal? TargetAmount { get; init; }
    public decimal CurrentBalance { get; init; }
    public string? Icon { get; init; }
    public string? Color { get; init; }
    public DateTime CreatedAt { get; init; }
    public double? ProgressPercentage { get; init; }
}
