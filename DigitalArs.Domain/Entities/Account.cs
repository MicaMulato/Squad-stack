namespace DigitalArs.Domain.Entities;

public class Account : BaseEntity
{
    public decimal Money { get; set; }
    public bool IsBlocked { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // CVU bancario único e inmodificable (22 dígitos estándar)
    public string Cvu { get; set; } = string.Empty;

    // Alias bancario único y modificable (ej. nombre.apellido.ars)
    public string Alias { get; set; } = string.Empty;

    // Foreign Key y propiedad de navegación bidireccional con User
    public int UserId { get; set; }
    public User? User { get; set; }

    // Propiedad de navegación bidireccional con Transaction
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    // Propiedad de navegación bidireccional con FixedTermDeposit (HU-33)
    public ICollection<FixedTermDeposit> FixedTermDeposits { get; set; } = new List<FixedTermDeposit>();

    // Propiedad de navegación bidireccional con Card (HU-35)
    public ICollection<Card> Cards { get; set; } = new List<Card>();
}
