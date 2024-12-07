#region

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using zora.Core.Domain;
using zora.Core.Interfaces;
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

    public async Task<bool> CreateAsync(T entity)
    {
        await this.DbSet.AddAsync(entity);
        return await this.DbContext.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(T entity)
    {
        this.DbSet.Remove(entity);
        return await this.DbContext.SaveChangesAsync() > 0;
    }
}
