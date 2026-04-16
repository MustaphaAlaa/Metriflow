using System.Linq.Expressions;
using Metriflow.Domain.Entities;

namespace IRepository;

public interface IUow
{


    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();

    Task<int> SaveChangesAsync();
}
