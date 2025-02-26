namespace zora.Core.Domain;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
