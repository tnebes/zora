namespace zora.Core.Domain;

public class User : BaseEntity
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ICollection<Role> Roles { get; set; } = new List<Role>();
    public ICollection<WorkItem> AssignedWorkItems { get; set; } = new List<WorkItem>();
    public ICollection<WorkItem> CreatedWorkItems { get; set; } = new List<WorkItem>();
    public ICollection<WorkItem> UpdatedWorkItems { get; set; } = new List<WorkItem>();
}
