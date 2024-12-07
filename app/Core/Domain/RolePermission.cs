#region

using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace zora.Core.Domain;

[Table("zora_role_permissions")]
public class RolePermission : BaseCompositeEntity
{
    [Column("role_id")] public long RoleId { get; set; }

    [Column("permission_id")] public long PermissionId { get; set; }

    [ForeignKey("RoleId")] public virtual Role Role { get; set; } = null!;

    [ForeignKey("PermissionId")] public virtual Permission Permission { get; set; } = null!;
}
