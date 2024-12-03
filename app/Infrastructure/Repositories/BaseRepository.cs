#region

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using zora.Core.Domain;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.Repositories;

public abstract class BaseRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext DbContext;
    protected readonly DbSet<T> DbSet;
    protected readonly ILogger<BaseRepository<T>> Logger;

    protected BaseRepository(
        ApplicationDbContext dbContext,
        ILogger<BaseRepository<T>> logger)
    {
        this.DbContext = dbContext;
        this.DbSet = dbContext.Set<T>();
        this.Logger = logger;
    }

    protected virtual async Task<T?> GetByIdAsync(long id)
    {
        try
        {
            return await this.DbSet.FindAsync(id);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error retrieving entity of type {EntityType} with ID {Id}", typeof(T).Name, id);
            throw;
        }
    }

    protected virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            return await this.DbSet.ToListAsync();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error retrieving all entities of type {EntityType}", typeof(T).Name);
            throw;
        }
    }

    protected virtual async Task<T> AddAsync(T entity)
    {
        try
        {
            await this.DbSet.AddAsync(entity);
            await this.DbContext.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error adding entity of type {EntityType}", typeof(T).Name);
            throw;
        }
    }

    protected virtual async Task UpdateAsync(T entity)
    {
        try
        {
            this.DbContext.Entry(entity).State = EntityState.Modified;
            await this.DbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error updating entity of type {EntityType}", typeof(T).Name);
            throw;
        }
    }

    protected virtual async Task DeleteAsync(long id)
    {
        try
        {
            T? entity = await this.DbSet.FindAsync(id);
            if (entity != null)
            {
                this.DbSet.Remove(entity);
                await this.DbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error deleting entity of type {EntityType} with ID {Id}", typeof(T).Name, id);
            throw;
        }
    }

    protected virtual IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression) =>
        this.DbSet.Where(expression);

    protected virtual IQueryable<T> GetQueryable() => this.DbSet.AsQueryable();
}
