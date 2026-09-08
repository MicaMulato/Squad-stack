namespace DigitalArs.Application.DTOs.Accounts;

/// <summary>
/// Datos públicos para confirmación de destinatario en transferencias bancarias.
/// </summary>
public record AccountLookupResponse
{
    public int AccountId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Cvu { get; init; } = string.Empty;
    public string Alias { get; init; } = string.Empty;
    public string Bank { get; init; } = "DigitalArs Billetera Virtual";
    public string EmailMasked { get; init; } = string.Empty;
    public bool IsBlocked { get; init; }
}
