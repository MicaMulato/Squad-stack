using DigitalArs.Domain.Enums;

namespace DigitalArs.Application.DTOs;

/// <summary>
/// Parámetros de consulta para el historial de transacciones.
/// Se bindean desde la query string: GET /api/transactions/me?page=1&amp;pageSize=10&amp;type=1
/// </summary>
public class TransactionQueryDto
{
    /// <summary>Número de página (base 1). Por defecto: 1.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Cantidad de items por página. Por defecto: 10.</summary>
    public int PageSize { get; set; } = 10;

    /// <summary>Filtro opcional por tipo de transacción (1=Deposit, 2=TransferIn, 3=TransferOut).</summary>
    public TransactionType? Type { get; set; }

    /// <summary>Filtro opcional: fecha mínima (inclusive).</summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>Filtro opcional: fecha máxima (inclusive).</summary>
    public DateTime? DateTo { get; set; }

    /// <summary>Filtro opcional: monto mínimo (inclusive).</summary>
    public decimal? AmountMin { get; set; }

    /// <summary>Filtro opcional: monto máximo (inclusive).</summary>
    public decimal? AmountMax { get; set; }
}
