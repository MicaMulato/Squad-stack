namespace Domain.Entities;

using Domain.Enums;

public class Transaction : BaseEntity
{
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public string? Concept { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;

    // Foreign Key y propiedad de navegación con Account (cuenta origen)
    public int AccountId { get; set; }
    public Account? Account { get; set; }

    // Foreign Key y propiedad de navegación opcional con ToAccount (cuenta destino)
    public int? ToAccountId { get; set; }
    public Account? ToAccount { get; set; }
}
