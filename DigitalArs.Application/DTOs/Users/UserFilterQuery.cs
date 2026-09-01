using DigitalArs.Application.DTOs.Common;

namespace DigitalArs.Application.DTOs.Users;

/// <summary>
/// Filtros del listado paginado de usuarios (HU-12): nombre, email, rol y estado activo.
/// Hereda Page y PageSize de PaginationQuery.
/// </summary>
public record UserFilterQuery : PaginationQuery
{
    /// <summary>Filtro por nombre o apellido (coincidencia parcial).</summary>
    public string? Name { get; init; }

    /// <summary>Filtro por email (coincidencia parcial).</summary>
    public string? Email { get; init; }

    /// <summary>Filtro por rol exacto ("Admin" o "User").</summary>
    public string? Role { get; init; }

    /// <summary>Filtro por estado: true = activos, false = dados de baja, null = todos.</summary>
    public bool? IsActive { get; init; }
}
