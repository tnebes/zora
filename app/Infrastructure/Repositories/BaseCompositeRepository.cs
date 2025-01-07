#region

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using zora.Core.Domain;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.Repositories;

public abstract class BaseCompositeRepository<T> where T : BaseCompositeEntity
{
    protected readonly ApplicationDbContext DbContext;
    protected readonly DbSet<T> DbSet;
    protected readonly ILogger<BaseCompositeRepository<T>> Logger;

    protected BaseCompositeRepository(ApplicationDbContext dbContext, ILogger<BaseCompositeRepository<T>> logger)
    {
        this.DbContext = dbContext;
        this.DbSet = dbContext.Set<T>();
        this.Logger = logger;
    }

    protected IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression) => this.DbSet.Where(expression);

    public async Task<T> CreateAsync(T entity)
    {
        await this.DbSet.AddAsync(entity);
        await this.DbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(T entity)
    {
        this.DbSet.Remove(entity);
        return await this.DbContext.SaveChangesAsync() > 0;
    }

    protected async Task<bool> ExecuteInTransactionAsync(Func<Task> action)
    {
        IExecutionStrategy strategy = this.DbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using IDbContextTransaction transaction = await this.DbContext.Database.BeginTransactionAsync();
            try
            {
                await action();
                await this.DbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                this.Logger.LogError(ex, "Error executing transaction");
                return false;
            }
        });
    }
}
