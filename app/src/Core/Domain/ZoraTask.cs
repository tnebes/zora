namespace zora.Core.Domain;

public class ZoraTask : WorkItem
{
    public long? ProjectId { get; set; }

    public string? Priority { get; set; }

    public long? ParentTaskId { get; set; }

    public virtual Project? Project { get; set; }

    public virtual ZoraTask? ParentTask { get; set; }

    public virtual ICollection<ZoraTask> SubTasks { get; set; } = new List<ZoraTask>();
}
