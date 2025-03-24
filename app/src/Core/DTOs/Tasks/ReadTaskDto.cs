#region

using zora.Core.DTOs.WorkItems;

#endregion

namespace zora.Core.DTOs.Tasks;

public sealed class ReadTaskDto : WorkItemDto
{
    public long? ProjectId { get; set; }
    public string? Priority { get; set; }
    public long? ParentTaskId { get; set; }
}
