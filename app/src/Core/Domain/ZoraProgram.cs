namespace zora.Core.Domain;

public class ZoraProgram : WorkItem
{
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
