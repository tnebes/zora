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

    public Task<Result<(IEnumerable<ZoraTask>, int TotalCount)>> SearchAsync(DynamicQueryTaskParamsDto searchParams,
        bool includeProperties = false)
    {
        try
        {
            List<ZoraTask> dummyTasks = new List<ZoraTask>
            {
                new()
                {
                    Id = 1,
                    Name = "Task 1",
                    Description = "Dummy repository task 1",
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    ProjectId = 1,
                    Priority = "Medium",
                    AssigneeId = 1,
                    Assignee = includeProperties ? new User { Id = 1, Username = "john.doe" } : null
                },
                new()
                {
                    Id = 2,
                    Name = "Task 2",
                    Description = "Dummy repository task 2",
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    ProjectId = 1,
                    Priority = "High",
                    AssigneeId = 2,
                    Assignee = includeProperties ? new User { Id = 2, Username = "jane.doe" } : null
                }
            };

            return Task.FromResult(Result.Ok((dummyTasks.AsEnumerable(), dummyTasks.Count)));
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error searching tasks");
            return Task.FromResult(Result.Fail<(IEnumerable<ZoraTask>, int)>(Constants.ERROR_500_MESSAGE));
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
}
