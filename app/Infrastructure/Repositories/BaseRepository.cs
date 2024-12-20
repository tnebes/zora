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

    protected virtual IQueryable<T> FilteredDbSet => this.DbSet.Where(entity => !EF.Property<bool>(entity, "Deleted"));

    protected virtual async Task<T?> GetByIdAsync(long id)
    {
        try
        {
            return await this.FilteredDbSet.FirstOrDefaultAsync(e => e.Id == id);
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
            return await this.FilteredDbSet.ToListAsync();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error retrieving all entities of type {EntityType}", typeof(T).Name);
            throw;
        }
    }

    protected virtual async Task<IEnumerable<T>> GetAllAsync(int page, int pageSize)
    {
        try
        {
            return await this.FilteredDbSet.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex,
                "Error retrieving all entities of type {EntityType} on page {Page} with page size {PageSize}",
                typeof(T).Name, page, pageSize);
            throw;
        }
    }

    protected async Task<(IEnumerable<T>, int totalCount)> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            int totalCount = await this.FilteredDbSet.CountAsync();
            IEnumerable<T> entities = await this.FilteredDbSet.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (entities, totalCount);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex,
                "Error retrieving paged entities of type {EntityType} on page {Page} with page size {PageSize}",
                typeof(T).Name, page, pageSize);
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
        this.FilteredDbSet.Where(expression);

    protected virtual IQueryable<T> GetQueryable() => this.FilteredDbSet.AsQueryable();
}
