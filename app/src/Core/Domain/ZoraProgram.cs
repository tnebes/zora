namespace zora.Core.Domain;

public class ZoraProgram : WorkItem
{
    public string? Description { get; set; }

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
