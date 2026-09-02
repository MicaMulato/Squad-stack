using DigitalArs.Application.DTOs.Common;
using DigitalArs.Domain.Enums;

namespace DigitalArs.Application.DTOs.Transactions;

/// <summary>
/// Filtros del historial de transacciones (HU-17): tipo, rango de fechas y rango de
/// monto. Paginado y ordenado descendente por fecha. Hereda Page y PageSize.
/// </summary>
public record TransactionFilterQuery : PaginationQuery
{
    /// <summary>Filtro por tipo de movimiento. Null = todos.</summary>
    public TransactionType? Type { get; init; }

    /// <summary>Fecha desde (inclusive, UTC).</summary>
    public DateTime? DateFrom { get; init; }

    /// <summary>Fecha hasta (inclusive, UTC).</summary>
    public DateTime? DateTo { get; init; }

    /// <summary>Monto minimo (inclusive).</summary>
    public decimal? MinAmount { get; init; }

    /// <summary>Monto maximo (inclusive).</summary>
    public decimal? MaxAmount { get; init; }
}
