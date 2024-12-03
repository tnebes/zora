namespace zora.Core.Domain;

public class Project : WorkItem
{
    public long? ProgramId { get; set; }
    public ZoraProgram? Program { get; set; }
    public long? ProjectManagerId { get; set; }
    public User? ProjectManager { get; set; }
    public ICollection<ZoraTask> Tasks { get; set; } = new List<ZoraTask>();
}
