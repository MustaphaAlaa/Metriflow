using System.Linq.Expressions;
using Metriflow.Domain.Entities;

namespace IRepository.Generic;

public interface IBaseRepository<TEntity>
    where TEntity : class
{
    Task<TEntity> CreateAsync(TEntity entity);
    Task CreateRange(IEnumerable<TEntity> entities);

    Task DeleteAsync(Expression<Func<TEntity, bool>> predicate);
    Task<List<TEntity>> RetrieveAllAsync();
    Task<IEnumerable<TEntity>> RetrieveAllAsync(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity?> RetrieveAsync(Expression<Func<TEntity, bool>> predicate);

    Task<TEntity?> RetrieveWithIncludeAsync(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, object>> includePredicate
    );

    TEntity Update(TEntity entity);
    void UpdateRange(IEnumerable<TEntity> entities);

    Task<TEntity?> RetrieveTrackedAsync(Expression<Func<TEntity, bool>> predicate);

    Task<List<TEntity>> RetrieveAllTrackedAsync();
    Task<List<TEntity>> RetrieveAllTrackedAsync(Expression<Func<TEntity, bool>> predicate);
}