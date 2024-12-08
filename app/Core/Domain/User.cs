namespace zora.Core.Domain;

public class User : BaseEntity
{
    public long Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<WorkItem> AssignedWorkItems { get; set; } = new List<WorkItem>();

    public virtual ICollection<WorkItem> CreatedWorkItems { get; set; } = new List<WorkItem>();

    public virtual ICollection<WorkItem> UpdatedWorkItems { get; set; } = new List<WorkItem>();

    public virtual ICollection<Project> ManagedProjects { get; set; } = new List<Project>();
}
