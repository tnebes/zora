#region

using zora.Core.Domain;
using zora.Core.Enums;
using zora.Infrastructure.Data;
using TaskStatus = zora.Core.Enums.TaskStatus;

#endregion

namespace zora.Tests.Utils;

public sealed class TaskUtils
{
    private ApplicationDbContext dbContext;

    public TaskUtils(ApplicationDbContext dbContext) => this.dbContext = dbContext;

    public List<ZoraTask> GetValidTasks()
    {
        return new List<ZoraTask>
        {
            this.GetValidTask(),
            this.GetValidTask(),
            this.GetValidTask(),
            this.GetValidTask()
        };
    }

    public ZoraTask GetValidTask()
    {
        return new ZoraTask
        {
            Name = "Valid Task",
            Description = "A valid task description",
            Status = TaskStatus.InProgress.ToString(),
            StartDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(7),
            CompletionPercentage = 0,
            EstimatedHours = 8,
            ActualHours = 0,
            Priority = TaskPriority.Medium.ToString(),
            ProjectId = 1
        };
    }

    public List<ZoraTask> GetInvalidTasks()
    {
        return new List<ZoraTask>
        {
            this.GetInvalidTask(),
            this.GetInvalidTask(),
            this.GetInvalidTask()
        };
    }

    public ZoraTask GetInvalidTask()
    {
        return new ZoraTask
        {
            Name = string.Empty,
            Description = null,
            Status = string.Empty,
            StartDate = null,
            DueDate = null,
            CompletionPercentage = -1,
            EstimatedHours = -5,
            ActualHours = -2,
            Priority = null,
            ProjectId = -1
        };
    }
}
