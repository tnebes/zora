namespace zora.Core.Domain;

public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string PermissionString { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    public virtual ICollection<PermissionWorkItem> PermissionWorkItems { get; set; } = new List<PermissionWorkItem>();
}
