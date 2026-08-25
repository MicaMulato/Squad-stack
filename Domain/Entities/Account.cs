namespace Domain.Entities;

public class Account
{
    public int Id 
    { get; set; }
    public decimal Money 
    { get; set; }
    public bool IsBlocked 
    { get; set; } = false;
    public string Cbu { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;

    // Foreign Key y propiedad de navegación bidireccional con User
    public int UserId { get; set; }
    public User? User { get; set; }

    // Propiedad de navegación bidireccional con Transaction
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
