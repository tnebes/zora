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
                    Priority = "Medium"
                },
                new()
                {
                    Id = 2,
                    Name = "Task 2",
                    Description = "Dummy repository task 2",
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    ProjectId = 1,
                    Priority = "High"
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
}
