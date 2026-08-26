namespace DigitalArs.Domain.Entities
{

    /*
     * Clase base para todas las entidades del dominio.
     * Contiene propiedades comunes que todas las entidades deben tener (Id).
     * es necesaria para que (IRepository<T>) funcione correctamente en cualquier entidad.
     */
    public abstract class BaseEntity
    {
        // identificador unico de la entidad (Primary Key)
        public int Id { get; set; }
    }
}
