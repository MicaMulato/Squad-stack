namespace DigitalArs.Application.DTOs;

/// <summary>
/// Wrapper genérico para respuestas paginadas.
/// Incluye los items de la página actual y la metadata de paginación.
/// Es reutilizable para cualquier tipo de entidad.
/// </summary>
public class PagedResultDto<T>
{
    /// <summary>Items de la página actual.</summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    /// <summary>Página actual (base 1).</summary>
    public int Page { get; set; }

    /// <summary>Cantidad de items por página solicitada.</summary>
    public int PageSize { get; set; }

    /// <summary>Total de items que coinciden con los filtros (sin paginar).</summary>
    public int TotalItems { get; set; }

    /// <summary>Total de páginas = CEIL(TotalItems / PageSize).</summary>
    public int TotalPages { get; set; }
}
