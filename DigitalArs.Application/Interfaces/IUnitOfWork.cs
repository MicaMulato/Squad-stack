using DigitalArs.Domain.Entities;
namespace DigitalArs.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IBaseRepository<T> Repository<T>() where T : BaseEntity;
        //Excepcion para User
        IBaseRepository<User> Users { get; }

        Task<int> SaveChangesAsync();

        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
