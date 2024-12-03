#region

using Microsoft.EntityFrameworkCore;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.Interfaces;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.Repositories;

[ServiceLifetime(ServiceLifetime.Scoped)]
public class WorkItemRepository : BaseRepository<WorkItem>, IWorkItemRepository
{
    public WorkItemRepository(
        ApplicationDbContext dbContext,
        ILogger<WorkItemRepository> logger)
        : base(dbContext, logger)
    {
    }

    public async Task<WorkItem?> GetWorkItemByIdAsync(long id)
    {
        try
        {
            return await this.DbSet
                .Include(w => w.Assignee)
                .Include(w => w.CreatedBy)
                .Include(w => w.UpdatedBy)
                .Include(w => w.Permissions)
                .Include(w => w.Assets)
                .Include(w => w.SourceRelationships)
                .ThenInclude(r => r.TargetItem)
                .Include(w => w.TargetRelationships)
                .ThenInclude(r => r.SourceItem)
                .FirstOrDefaultAsync(w => w.Id == id);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error retrieving work item with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<WorkItem>> GetAllWorkItemsAsync()
    {
        try
        {
            return await this.DbSet
                .Include(w => w.Assignee)
                .Include(w => w.CreatedBy)
                .Include(w => w.UpdatedBy)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error retrieving all work items");
            throw;
        }
    }

    public async Task<WorkItem> CreateWorkItemAsync(WorkItem workItem)
    {
        try
        {
            await this.DbSet.AddAsync(workItem);
            await this.DbContext.SaveChangesAsync();
            return workItem;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error creating work item");
            throw;
        }
    }

    public async Task UpdateWorkItemAsync(WorkItem workItem)
    {
        try
        {
            this.DbContext.Entry(workItem).State = EntityState.Modified;
            await this.DbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error updating work item with ID {Id}", workItem.Id);
            throw;
        }
    }

    public async Task DeleteWorkItemAsync(long id)
    {
        try
        {
            WorkItem? workItem = await this.DbSet.FindAsync(id);
            if (workItem != null)
            {
                this.DbSet.Remove(workItem);
                await this.DbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error deleting work item with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ZoraProgram>> GetAllProgramsAsync()
    {
        try
        {
            return await this.DbContext.Set<ZoraProgram>()
                .Include(p => p.Assignee)
                .Include(p => p.CreatedBy)
                .Include(p => p.UpdatedBy)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error retrieving all programs");
            throw;
        }
    }

    public async Task<ZoraProgram?> GetProgramByIdAsync(long id)
    {
        try
        {
            return await this.DbContext.Set<ZoraProgram>()
                .Include(p => p.Assignee)
                .Include(p => p.CreatedBy)
                .Include(p => p.UpdatedBy)
                .Include(p => p.Permissions)
                .Include(p => p.Assets)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error retrieving program with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Project>> GetAllProjectsAsync()
    {
        try
        {
            return await this.DbContext.Set<Project>()
                .Include(p => p.Assignee)
                .Include(p => p.CreatedBy)
                .Include(p => p.UpdatedBy)
                .Include(p => p.Program)
                .Include(p => p.ProjectManager)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error retrieving all projects");
            throw;
        }
    }

    public async Task<Project?> GetProjectByIdAsync(long id)
    {
        try
        {
            return await this.DbContext.Set<Project>()
                .Include(p => p.Assignee)
                .Include(p => p.CreatedBy)
                .Include(p => p.UpdatedBy)
                .Include(p => p.Program)
                .Include(p => p.ProjectManager)
                .Include(p => p.Permissions)
                .Include(p => p.Assets)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error retrieving project with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Project>> GetProjectsByProgramIdAsync(long programId)
    {
        try
        {
            return await this.DbContext.Set<Project>()
                .Include(p => p.Assignee)
                .Include(p => p.CreatedBy)
                .Include(p => p.UpdatedBy)
                .Include(p => p.Program)
                .Include(p => p.ProjectManager)
                .Where(p => p.ProgramId == programId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error retrieving projects for program ID {ProgramId}", programId);
            throw;
        }
    }

    public async Task<IEnumerable<ZoraTask>> GetAllTasksAsync()
    {
        try
        {
            return await this.DbContext.Set<ZoraTask>()
                .Include(t => t.Assignee)
                .Include(t => t.CreatedBy)
                .Include(t => t.UpdatedBy)
                .Include(t => t.Project)
                .Include(t => t.ParentTask)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error retrieving all tasks");
            throw;
        }
    }

    public async Task<ZoraTask?> GetTaskByIdAsync(long id)
    {
        try
        {
            return await this.DbContext.Set<ZoraTask>()
                .Include(t => t.Assignee)
                .Include(t => t.CreatedBy)
                .Include(t => t.UpdatedBy)
                .Include(t => t.Project)
                .Include(t => t.ParentTask)
                .Include(t => t.Permissions)
                .Include(t => t.Assets)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error retrieving task with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ZoraTask>> GetTasksByProjectIdAsync(long projectId)
    {
        try
        {
            return await this.DbContext.Set<ZoraTask>()
                .Include(t => t.Assignee)
                .Include(t => t.CreatedBy)
                .Include(t => t.UpdatedBy)
                .Include(t => t.Project)
                .Include(t => t.ParentTask)
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error retrieving tasks for project ID {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<IEnumerable<ZoraTask>> GetTasksByAssigneeIdAsync(long assigneeId)
    {
        try
        {
            return await this.DbContext.Set<ZoraTask>()
                .Include(t => t.Assignee)
                .Include(t => t.CreatedBy)
                .Include(t => t.UpdatedBy)
                .Include(t => t.Project)
                .Include(t => t.ParentTask)
                .Where(t => t.AssigneeId == assigneeId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error retrieving tasks for assignee ID {AssigneeId}", assigneeId);
            throw;
        }
    }
}
