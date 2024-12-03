#region

using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces;

public interface IWorkItemRepository : IZoraService
{
    Task<IEnumerable<WorkItem>> GetByAssigneeIdAsync(long assigneeId);
    Task<IEnumerable<WorkItem>> GetByStatusAsync(string status);
    Task<IEnumerable<WorkItem>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}
