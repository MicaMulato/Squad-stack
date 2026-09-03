using DigitalArs.Domain.Entities;
using System.Linq.Expressions;

namespace DigitalArs.Application.Interfaces
{
    public interface IBaseRepository<T> where T: class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Devuelve un IQueryable sin ejecutar para componer filtros, proyecciones
        /// y paginación en la base de datos (sin traer entidades a memoria primero).
        /// </summary>
        IQueryable<T> Query();

        Task AddAsync (T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
