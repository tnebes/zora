#region

using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace zora.Core.Domain;

[Table("zora_user_roles")]
public class UserRole
{
    [Column("user_id")] public long UserId { get; set; }

    [Column("role_id")] public long RoleId { get; set; }

    [ForeignKey("UserId")] public virtual User User { get; set; } = null!;

    [ForeignKey("RoleId")] public virtual Role Role { get; set; } = null!;
}
