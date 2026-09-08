namespace DigitalArs.Domain.Entities;

public class Account : BaseEntity
{
    public decimal Money { get; set; }
    public bool IsBlocked { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Key y propiedad de navegación bidireccional con User
    public int UserId { get; set; }
    public User? User { get; set; }

    // Propiedad de navegación bidireccional con Transaction
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    // Propiedad de navegación bidireccional con FixedTermDeposit (HU-33)
    public ICollection<FixedTermDeposit> FixedTermDeposits { get; set; } = new List<FixedTermDeposit>();

    // Propiedad de navegación bidireccional con Card (HU-35)
    public ICollection<Card> Cards { get; set; } = new List<Card>();

    // Propiedad de navegación bidireccional con MoneyReserve
    public ICollection<MoneyReserve> Reserves { get; set; } = new List<MoneyReserve>();

    // Propiedad de navegación bidireccional con ServicePayment
    public ICollection<ServicePayment> ServicePayments { get; set; } = new List<ServicePayment>();
}
