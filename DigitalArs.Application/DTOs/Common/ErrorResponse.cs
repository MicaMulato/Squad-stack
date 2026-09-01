namespace DigitalArs.Application.DTOs.Common;

/// <summary>
/// Formato unico de error de la API (HU-18). El middleware global de errores
/// serializa toda excepcion a esta forma para respuestas consistentes en el frontend.
/// </summary>
public record ErrorResponse
{
    public int StatusCode { get; init; }
    public string Message { get; init; } = string.Empty;

    /// <summary>Lista de errores de detalle (por ejemplo, errores de validacion por campo).</summary>
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    /// <summary>Identificador de traza para correlacionar la respuesta con los logs.</summary>
    public string? TraceId { get; init; }
}
