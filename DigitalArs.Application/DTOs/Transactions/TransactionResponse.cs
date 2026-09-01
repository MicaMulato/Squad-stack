namespace DigitalArs.Application.DTOs.Transactions;

/// <summary>
/// Item del historial de movimientos (HU-17) y de los ultimos movimientos del
/// dashboard (HU-24). Proyeccion plana para evitar N+1: la contraparte se resuelve
/// en la consulta con un Select, no cargando entidades completas.
/// </summary>
public record TransactionResponse
{
    public int Id { get; init; }
    public decimal Amount { get; init; }

    /// <summary>Tipo de movimiento como texto: "Deposit", "TransferIn", "TransferOut".</summary>
    public string Type { get; init; } = string.Empty;

    public string? Concept { get; init; }
    public DateTime Date { get; init; }

    /// <summary>
    /// Nombre de la contraparte (origen o destino segun el tipo). Null en depositos.
    /// </summary>
    public string? CounterpartyName { get; init; }
}
