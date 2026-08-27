using DigitalArs.Domain.Entities;
using System.Linq.Expressions;

namespace DigitalArs.Application.Interfaces
{
    public interface IRepository<T> where T: BaseEntity
    {
        Task<T?> GetAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync (T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
