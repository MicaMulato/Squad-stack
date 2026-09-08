using DigitalArs.Domain.Enums;

namespace DigitalArs.Domain.Entities;

public class ServiceProvider : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ServiceCategory Category { get; set; }
    public string CodeLabel { get; set; } = string.Empty;
    public string? IconName { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ServicePayment> Payments { get; set; } = new List<ServicePayment>();
}
