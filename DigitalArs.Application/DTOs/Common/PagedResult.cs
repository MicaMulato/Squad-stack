namespace DigitalArs.Application.DTOs.Common;

/// <summary>
/// Resultado paginado generico con metadata de paginacion.
/// Usado en listados como usuarios (HU-12) e historial de transacciones (HU-17).
/// </summary>
/// <typeparam name="T">Tipo del item (normalmente un DTO de response).</typeparam>
public record PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }

    /// <summary>Cantidad total de paginas, calculada a partir de TotalItems y PageSize.</summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalItems / (double)PageSize) : 0;

    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public PagedResult() { }

    public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalItems)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalItems = totalItems;
    }
}
