using IRepository.Generic;
using Entities.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Repositories.Generic;

public class UnitOfWork : IUnitOfWork
{
    private readonly KemetDbContext _db;
    private Dictionary<Type, object> _repositories;
    private IDbContextTransaction _transaction;

    public UnitOfWork(KemetDbContext context)
    {
        _db = context;
        _repositories = new();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _db.SaveChangesAsync();
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    public IBaseRepository<T> GetRepository<T>()
        where T : class
    {
        var type = typeof(T);
        if (!_repositories.ContainsKey(type))
        {
            var repo = new BaseRepository<T>(_db);
            _repositories.Add(type, repo);
        }

        return (IBaseRepository<T>)_repositories[type];
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _db.Database.BeginTransactionAsync();
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
