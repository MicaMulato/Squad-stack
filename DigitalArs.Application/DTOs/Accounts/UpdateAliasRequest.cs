using System.ComponentModel.DataAnnotations;

namespace DigitalArs.Application.DTOs.Accounts;

/// <summary>
/// Solicitud para modificar el alias bancario de la cuenta del usuario.
/// </summary>
public record UpdateAliasRequest
{
    [Required(ErrorMessage = "El alias es requerido.")]
    [StringLength(50, MinimumLength = 4, ErrorMessage = "El alias debe tener entre 4 y 50 caracteres.")]
    [RegularExpression(@"^[a-zA-Z0-9.\-_]+$", ErrorMessage = "El alias solo puede contener letras, números, puntos, guiones y guiones bajos.")]
    public string Alias { get; init; } = string.Empty;
}
