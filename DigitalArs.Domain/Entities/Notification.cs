namespace DigitalArs.Domain.Entities;

public class Notification : BaseEntity
{
    public int UserId { get; set; }
    public User? User { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "General"; // "Welcome", "Deposit", "TransferIn", "General"

    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? ActionUrl { get; set; }
}
