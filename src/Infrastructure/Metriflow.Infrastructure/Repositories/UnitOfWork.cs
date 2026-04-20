using IRepository;
using Metriflow.Infrastructure;


public class UnitOfWork(MetriflowDbContext context) : IUow
{
    public async Task BeginTransactionAsync()
    {
        await context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await context.Database.CommitTransactionAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        await context.Database.RollbackTransactionAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await context.SaveChangesAsync();
    }
}