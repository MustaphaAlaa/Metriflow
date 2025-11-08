namespace IRepository.Generic;

public interface IUnitOfWork : IDisposable
{
    IBaseRepository<T> GetRepository<T>()
        where T : class;
    Task<int> SaveChangesAsync(); // Use async for best practice
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
