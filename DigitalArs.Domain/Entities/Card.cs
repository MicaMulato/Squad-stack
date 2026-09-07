using DigitalArs.Domain.Enums;

namespace DigitalArs.Domain.Entities;

public class Card : BaseEntity
{
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;

    /// <summary>Nombre del titular en mayúsculas (e.g. "EMMA PORRETTI")</summary>
    public string HolderName { get; set; } = string.Empty;

    /// <summary>Número de tarjeta de 16 dígitos (formato Visa: 4XXX XXXX XXXX XXXX)</summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>Código de seguridad de 3 dígitos (CVV)</summary>
    public string SecurityCode { get; set; } = string.Empty;

    /// <summary>Fecha de vencimiento (3 años desde la creación)</summary>
    public DateTime ExpirationDate { get; set; }

    /// <summary>Tipo de tarjeta: Virtual o Physical</summary>
    public CardType Type { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Tarjeta activa para operar</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Tarjeta congelada temporalmente por el usuario</summary>
    public bool IsFrozen { get; set; } = false;
}
