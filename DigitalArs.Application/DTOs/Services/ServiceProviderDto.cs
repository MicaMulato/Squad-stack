using DigitalArs.Domain.Enums;

namespace DigitalArs.Application.DTOs.Services;

public record ServiceProviderDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public ServiceCategory Category { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string CodeLabel { get; init; } = string.Empty;
    public string? IconName { get; init; }
}
