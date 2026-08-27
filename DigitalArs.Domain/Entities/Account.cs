namespace DigitalArs.Domain.Entities;

public class Account : BaseEntity
{
    public decimal Money { get; set; }
    public bool IsBlocked { get; set; } = false;

    // Foreign Key y propiedad de navegación bidireccional con User
    public int UserId { get; set; }
    public User? User { get; set; }

    // Propiedad de navegación bidireccional con Transaction
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
