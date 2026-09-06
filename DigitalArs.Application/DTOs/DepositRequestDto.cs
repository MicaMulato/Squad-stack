namespace DigitalArs.Application.DTOs;

/// <summary>
/// Datos que el cliente envía en el body de POST /api/accounts/deposit.
/// </summary>
public class DepositRequestDto
{
    public decimal Amount { get; set; }
    public string? Concept { get; set; }
}
