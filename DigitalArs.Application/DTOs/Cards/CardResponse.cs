namespace DigitalArs.Application.DTOs.Cards;

public class CardResponse
{
    public int Id { get; set; }
    public string HolderName { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public string LastFourDigits { get; set; } = string.Empty;
    public string SecurityCode { get; set; } = string.Empty;
    public string ExpirationDate { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsFrozen { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}
