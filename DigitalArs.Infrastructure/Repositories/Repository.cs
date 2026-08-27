using System.Linq.Expressions;
using DigitalArs.Application.Interfaces;
using DigitalArs.Domain.Entities;
using DigitalArs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace DigitalArs.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : BaseEntity 
    {
        protected readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T?> GetAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
        public async Task AddAsync( T entity)
        {
            await _dbSet.AddAsync(entity);
        }
        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }
        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }
    }
}
