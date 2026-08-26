namespace DigitalArs.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> Repository<T>() where T : DigitalArs.Domain.Entities.BaseEntity;

        Task<int> SaveChangesAsync();

        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
