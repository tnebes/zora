namespace zora.Core.Domain;

public class Permission : BaseEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string PermissionString { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ICollection<Role> Roles { get; set; } = new List<Role>();
    public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}
