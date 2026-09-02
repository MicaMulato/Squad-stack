namespace DigitalArs.Application.DTOs.Common;

/// <summary>
/// Parametros base de paginacion para consultas de listado.
/// Los filtros especificos (usuarios, transacciones) heredan de esta clase.
/// </summary>
public record PaginationQuery
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;

    /// <summary>Numero de pagina (1-based). Minimo 1.</summary>
    public int Page { get; init; } = 1;

    /// <summary>Tamaño de pagina. Acotado entre 1 y 100 para proteger el servidor.</summary>
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value < 1 ? 1 : (value > MaxPageSize ? MaxPageSize : value);
    }
}
