namespace zora.Core.Domain;

public class Project : WorkItem
{
    public long? ProgramId { get; set; }

    public long? ProjectManagerId { get; set; }

    public virtual ZoraProgram? Program { get; set; }

    public virtual User? ProjectManager { get; set; }

    public virtual ICollection<ZoraTask> Tasks { get; set; } = new List<ZoraTask>();
}
