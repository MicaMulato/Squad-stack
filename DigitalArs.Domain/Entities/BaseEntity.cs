namespace DigitalArs.Domain.Entities;

/// <summary>
/// Clase base para todas las entidades del dominio.
/// Contiene propiedades comunes que todas las entidades deben tener (Id).
/// Es necesaria para que IRepository&lt;T&gt; funcione correctamente en cualquier entidad.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
}
