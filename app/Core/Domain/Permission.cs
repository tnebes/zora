#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace zora.Core.Domain;

[Table("zora_permissions")]
public class Permission : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Column("description")] public string? Description { get; set; }

    [Required]
    [Column("permission_string")]
    [StringLength(5, MinimumLength = 5)]
    [RegularExpression("^[0-1]{5}$", ErrorMessage = "Permission string must be exactly 5 binary digits (0 or 1)")]
    public string PermissionString { get; set; } = string.Empty;

    [Column("created_at")] public DateTime CreatedAt { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public virtual ICollection<PermissionWorkItem> PermissionWorkItems { get; set; } = new List<PermissionWorkItem>();
}
