#region

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using zora.Core;
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
            this.Logger.LogError(ex,
                "Error retrieving entity of type {EntityType} with ID {Id}. Exception: {ExceptionMessage}",
                typeof(T).Name, id, Constants.ERROR_500_MESSAGE);
            throw new InvalidOperationException($"Unable to retrieve entity of type {typeof(T).Name} with ID {id}", ex);
        }
    }

    protected virtual async Task<List<T>> GetAllAsync()
    {
        try
        {
            return await this.FilteredDbSet.ToListAsync();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex,
                "Error retrieving all entities of type {EntityType}. Exception: {ExceptionMessage}", typeof(T).Name,
                Constants.ERROR_500_MESSAGE);
            throw new InvalidOperationException($"Unable to retrieve all entities of type {typeof(T).Name}", ex);
        }
    }

    protected virtual IQueryable<T> GetAllAsync(int page, int pageSize)
    {
        try
        {
            return this.FilteredDbSet.Skip((page - 1) * pageSize).Take(pageSize);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex,
                "Error retrieving paged entities of type {EntityType}. Page: {Page}, PageSize: {PageSize}. Exception: {ExceptionMessage}",
                typeof(T).Name, page, pageSize, Constants.ERROR_500_MESSAGE);
            throw new InvalidOperationException(
                $"Unable to retrieve entities of type {typeof(T).Name} for page {page} with page size {pageSize}", ex);
        }
    }

    protected async Task<(IQueryable<T>, int totalCount)> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            return await this.GetPagedAsync(this.FilteredDbSet, page, pageSize);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex,
                "Error retrieving paged entities of type {EntityType}. Page: {Page}, PageSize: {PageSize}. Exception: {ExceptionMessage}",
                typeof(T).Name, page, pageSize, Constants.ERROR_500_MESSAGE);
            throw new InvalidOperationException(
                $"Unable to retrieve paged entities of type {typeof(T).Name} for page {page} with page size {pageSize}",
                ex);
        }
    }

    protected async Task<(IQueryable<T>, int totalCount)> GetPagedAsync(IQueryable<T> query, int page, int pageSize)
    {
        int totalCount = await query.CountAsync();

        int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        int adjustedPage = totalPages > 0 ? Math.Min(page, totalPages) : 1;

        IQueryable<T> entities = query.Skip((adjustedPage - 1) * pageSize).Take(pageSize);
        return (entities, totalCount);
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
            this.Logger.LogError(ex, "Error adding entity of type {EntityType}. Exception: {ExceptionMessage}",
                typeof(T).Name, Constants.ERROR_500_MESSAGE);
            throw new InvalidOperationException($"Unable to add entity of type {typeof(T).Name}", ex);
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
            this.Logger.LogError(ex, "Error updating entity of type {EntityType}. Exception: {ExceptionMessage}",
                typeof(T).Name, Constants.ERROR_500_MESSAGE);
            throw new InvalidOperationException($"Unable to update entity of type {typeof(T).Name}", ex);
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
            this.Logger.LogError(ex,
                "Error deleting entity of type {EntityType} with ID {Id}. Exception: {ExceptionMessage}",
                typeof(T).Name, id, Constants.ERROR_500_MESSAGE);
            throw new InvalidOperationException($"Unable to delete entity of type {typeof(T).Name} with ID {id}", ex);
        }
    }

    protected void ApplyFilter<TEntity, TValue>(
        ref IQueryable<TEntity> query,
        TValue value,
        Expression<Func<TEntity, TValue, bool>> predicate)
    {
        if (value != null)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(TEntity));
            InvocationExpression body = Expression.Invoke(predicate, parameter, Expression.Constant(value));
            query = query.Where(Expression.Lambda<Func<TEntity, bool>>(body, parameter));
        }
    }

    protected void ApplyListFilter<TEntity, TValue>(
        ref IQueryable<TEntity> query,
        string input,
        Func<string, TValue> parser,
        Expression<Func<TEntity, List<TValue>, bool>> predicate)
    {
        if (!string.IsNullOrWhiteSpace(input))
        {
            List<TValue> values = this.ParseStringToList(input, parser);
            ParameterExpression parameter = Expression.Parameter(typeof(TEntity));
            InvocationExpression body = Expression.Invoke(predicate, parameter, Expression.Constant(values));
            query = query.Where(Expression.Lambda<Func<TEntity, bool>>(body, parameter));
        }
    }

    protected List<T> ParseStringToList<T>(string input, Func<string, T> parser)
    {
        return input.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(parser)
            .ToList();
    }

    protected virtual IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression) =>
        this.FilteredDbSet.Where(expression);

    protected virtual IQueryable<T> GetQueryable() => this.FilteredDbSet.AsQueryable();

    protected IQueryable<T> FindAll() => this.FilteredDbSet;
}
