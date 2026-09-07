namespace DigitalArs.Application.DTOs.Accounts;

/// <summary>
/// Deposito de dinero en la cuenta propia (HU-15).
/// El monto debe ser mayor a cero; el limite maximo por operacion es configurable
/// y se valida en la capa de aplicacion/validador.
/// </summary>
public record DepositRequest
{
    public decimal Amount { get; init; }
    public string? Concept { get; init; }
}
