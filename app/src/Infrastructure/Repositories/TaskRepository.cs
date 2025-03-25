#region

using FluentResults;
using Microsoft.EntityFrameworkCore;
using zora.Core;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.Interfaces.Repositories;
using zora.Core.Interfaces.Services;
using zora.Core.Utils;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.Repositories;

[ServiceLifetime(ServiceLifetime.Scoped)]
public class TaskRepository : BaseRepository<ZoraTask>, ITaskRepository, IZoraService
{
    public TaskRepository(ApplicationDbContext dbContext, ILogger<TaskRepository> logger) : base(dbContext, logger)
    {
    }

    public async Task<Result<(IEnumerable<ZoraTask>, int TotalCount)>> SearchAsync(
        DynamicQueryTaskParamsDto searchParams,
        bool includeProperties = false)
    {
        try
        {
            IQueryable<ZoraTask> query = this.FilteredDbSet;

            if (includeProperties)
            {
                query = query
                    .Include(t => t.Assignee)
                    .Include(t => t.CreatedBy)
                    .Include(t => t.UpdatedBy);
            }

            if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm))
            {
                string searchTerm = $"%{searchParams.SearchTerm}%";
                query = query.Where(t =>
                    EF.Functions.Like(t.Name, searchTerm) ||
                    EF.Functions.Like(t.Description, searchTerm) ||
                    EF.Functions.Like(t.Status, searchTerm) ||
                    EF.Functions.Like(t.Priority, searchTerm) ||
                    (t.Assignee != null && EF.Functions.Like(t.Assignee.Username, searchTerm)) ||
                    (t.CreatedBy != null && EF.Functions.Like(t.CreatedBy.Username, searchTerm)) ||
                    (t.UpdatedBy != null && EF.Functions.Like(t.UpdatedBy.Username, searchTerm))
                );
            }

            if (!string.IsNullOrWhiteSpace(searchParams.Status))
            {
                query = query.Where(t => t.Status == searchParams.Status);
            }

            if (!string.IsNullOrWhiteSpace(searchParams.Priority))
            {
                query = query.Where(t => t.Priority == searchParams.Priority);
            }

            if (searchParams.AssigneeId.HasValue)
            {
                query = query.Where(t => t.AssigneeId == searchParams.AssigneeId);
            }

            if (searchParams.ProjectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == searchParams.ProjectId);
            }

            if (!string.IsNullOrWhiteSpace(searchParams.SortColumn))
            {
                query = ApplySorting(query, searchParams.SortColumn, searchParams.SortDirection);
            }
            else
            {
                query = query.OrderByDescending(t => t.UpdatedAt);
            }

            int totalCount = await query.CountAsync();
            int skip = (searchParams.Page - 1) * searchParams.PageSize;
            IEnumerable<ZoraTask> tasks = await query
                .Skip(skip)
                .Take(searchParams.PageSize)
                .ToListAsync();

            return Result.Ok((tasks, totalCount));
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error searching tasks");
            return Result.Fail<(IEnumerable<ZoraTask>, int)>(Constants.ERROR_500_MESSAGE);
        }
    }

    public async Task<Result<(IEnumerable<ZoraTask>, int total)>> GetPagedAsync(IQueryable<ZoraTask> query, int page,
        int pageSize)
    {
        try
        {
            int totalCount = await query.CountAsync();
            int totalPages = PaginationUtils.CalculateTotalPages(totalCount, pageSize);
            int adjustedPage = PaginationUtils.AdjustPage(page, totalPages);
            IQueryable<ZoraTask> entities =
                query.Skip(PaginationUtils.CalculateSkip(adjustedPage, pageSize)).Take(pageSize);
            return (entities, totalCount);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error getting paged tasks");
            return Result.Fail<(IEnumerable<ZoraTask>, int)>(Constants.ERROR_500_MESSAGE);
        }
    }

    public new IQueryable<ZoraTask> GetQueryable() => base.GetQueryable();

    public async Task<Result<ZoraTask>> GetByIdAsync(long id, bool includeProperties)
    {
        try
        {
            IQueryable<ZoraTask> query = this.FilteredDbSet.Where(t => t.Id == id);

            if (includeProperties)
            {
                query = query.Include(t => t.Assignee);
            }

            ZoraTask? task = await query.FirstOrDefaultAsync();

            if (task == null)
            {
                return Result.Fail<ZoraTask>($"Task with id {id} not found");
            }

            return Result.Ok(task);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error getting task by id {Id}", id);
            return Result.Fail<ZoraTask>($"Error getting task by id {id}");
        }
    }

    public new async Task<Result<ZoraTask>> UpdateAsync(ZoraTask task)
    {
        try
        {
            this.DbContext.Entry(task).State = EntityState.Modified;
            await this.DbContext.SaveChangesAsync();
            return Result.Ok(task);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error updating task with id {Id}", task.Id);
            return Result.Fail<ZoraTask>($"Error updating task with id {task.Id}");
        }
    }

    public async Task<Result<bool>> DeleteAsync(ZoraTask task)
    {
        try
        {
            task.Deleted = true;
            this.DbContext.Entry(task).State = EntityState.Modified;
            await this.DbContext.SaveChangesAsync();
            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error deleting task with id {Id}", task.Id);
            return Result.Fail<bool>($"Error deleting task with id {task.Id}");
        }
    }

    public async Task<Result<ZoraTask>> CreateAsync(ZoraTask task)
    {
        try
        {
            await this.DbContext.Tasks.AddAsync(task);
            await this.DbContext.SaveChangesAsync();
            return Result.Ok(task);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error creating task");
            return Result.Fail<ZoraTask>($"Error creating task: {ex.Message}");
        }
    }

    private static IQueryable<ZoraTask> ApplySorting(IQueryable<ZoraTask> query, string sortColumn,
        string? sortDirection)
    {
        bool isDescending = sortDirection?.ToLower() == "desc";

        return sortColumn.ToLower() switch
        {
            "id" => isDescending ? query.OrderByDescending(t => t.Id) : query.OrderBy(t => t.Id),
            "name" => isDescending ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name),
            "status" => isDescending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            "priority" => isDescending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "assignee" => isDescending
                ? query.OrderByDescending(t => t.Assignee != null ? t.Assignee.Username : string.Empty)
                : query.OrderBy(t => t.Assignee != null ? t.Assignee.Username : string.Empty),
            "duedate" => isDescending ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
            "completionpercentage" => isDescending
                ? query.OrderByDescending(t => t.CompletionPercentage)
                : query.OrderBy(t => t.CompletionPercentage),
            "createdat" => isDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
            "updatedat" => isDescending ? query.OrderByDescending(t => t.UpdatedAt) : query.OrderBy(t => t.UpdatedAt),
            "createdby" => isDescending
                ? query.OrderByDescending(t => t.CreatedBy != null ? t.CreatedBy.Username : string.Empty)
                : query.OrderBy(t => t.CreatedBy != null ? t.CreatedBy.Username : string.Empty),
            "updatedby" => isDescending
                ? query.OrderByDescending(t => t.UpdatedBy != null ? t.UpdatedBy.Username : string.Empty)
                : query.OrderBy(t => t.UpdatedBy != null ? t.UpdatedBy.Username : string.Empty),
            _ => query.OrderByDescending(t => t.UpdatedAt)
        };
    }
}
