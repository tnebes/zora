#region

using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces;

public interface IWorkItemRepository : IZoraService
{
    Task<WorkItem?> GetWorkItemByIdAsync(long id);
    Task<IEnumerable<WorkItem>> GetAllWorkItemsAsync();
    Task<WorkItem> CreateWorkItemAsync(WorkItem workItem);
    Task UpdateWorkItemAsync(WorkItem workItem);
    Task DeleteWorkItemAsync(long id);

    Task<IEnumerable<ZoraProgram>> GetAllProgramsAsync();
    Task<ZoraProgram?> GetProgramByIdAsync(long id);

    Task<IEnumerable<Project>> GetAllProjectsAsync();
    Task<Project?> GetProjectByIdAsync(long id);
    Task<IEnumerable<Project>> GetProjectsByProgramIdAsync(long programId);

    Task<IEnumerable<ZoraTask>> GetAllTasksAsync();
    Task<ZoraTask?> GetTaskByIdAsync(long id);
    Task<IEnumerable<ZoraTask>> GetTasksByProjectIdAsync(long projectId);
    Task<IEnumerable<ZoraTask>> GetTasksByAssigneeIdAsync(long assigneeId);
}
