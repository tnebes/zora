namespace zora.Core.Domain;

public class ZoraTask : WorkItem
{
    public long? ProjectId { get; set; }
    public Project? Project { get; set; }
    public string? Priority { get; set; }
    public long? ParentTaskId { get; set; }
    public ZoraTask? ParentTask { get; set; }
    public ICollection<ZoraTask> SubTasks { get; set; } = new List<ZoraTask>();
}
