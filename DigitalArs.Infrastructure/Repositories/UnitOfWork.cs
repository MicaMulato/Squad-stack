using DigitalArs.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using DigitalArs.Infrastructure.Data;
using DigitalArs.Domain.Entities;

namespace DigitalArs.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        // Cachea un repositorio por tipo, para no crear uno nuevo
        // cada vez que se pide el mismo T dentro de la misma unidad de trabajo
        private readonly Dictionary<Type, object> _repositories = new();

        private IDbContextTransaction? _currentTransaction;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }
        public IRepository<T> Repository<T>() where T : BaseEntity
        {
            var type = typeof(T);

            if (!_repositories.ContainsKey(type))
            {
                var repositoryInstance = new Repository<T>(_context);
                _repositories[type] = repositoryInstance;
            }
            return (IRepository<T>)_repositories[type];
        }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _currentTransaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();

                if (_currentTransaction != null)
                {
                    await _currentTransaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public void Dispose()
        {
            // El ApplicationDbContext es Scoped: su ciclo de vida lo gestiona el
            // contenedor de DI, que lo dispone al terminar la request. El UoW NO
            // debe disponerlo (lo comparte con UserManager/RoleManager de Identity);
            // hacerlo provocaria ObjectDisposedException. Solo disponemos lo que
            // el UoW crea: la transaccion en curso, si quedo abierta.
            _currentTransaction?.Dispose();
        }
    }
}
