using IRepository.Generic;
using Metriflow.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Repositories.Generic;

public class UnitOfWork(MetriflowDbContext context) : IUnitOfWork
{
    private Dictionary<Type, object> _repositories = new();
    private IDbContextTransaction _transaction;

    public async Task<int> SaveChangesAsync()
    {
        return await context.SaveChangesAsync();
    }

    public void Dispose()
    {
        context.Dispose();
    }

    public IBaseRepository<T> GetRepository<T>()
        where T : class
    {
        var type = typeof(T);
        if (!_repositories.ContainsKey(type))
        {
            var repo = new BaseRepository<T>(context);
            _repositories.Add(type, repo);
        }

        return (IBaseRepository<T>)_repositories[type];
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        await _transaction.CommitAsync();
    }

    public async Task RollbackAsync()
    {
        await _transaction.RollbackAsync();
    }
}
