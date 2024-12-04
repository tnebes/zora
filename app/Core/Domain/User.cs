#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace zora.Core.Domain;

[Table("zora_users")]
public class User : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("username")]
    [StringLength(255)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [Column("password")]
    [StringLength(255)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Column("email")]
    [StringLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Column("created_at")] public DateTime CreatedAt { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    [InverseProperty("Assignee")]
    public virtual ICollection<WorkItem> AssignedWorkItems { get; set; } = new List<WorkItem>();

    [InverseProperty("CreatedBy")]
    public virtual ICollection<WorkItem> CreatedWorkItems { get; set; } = new List<WorkItem>();

    [InverseProperty("UpdatedBy")]
    public virtual ICollection<WorkItem> UpdatedWorkItems { get; set; } = new List<WorkItem>();

    [InverseProperty("ProjectManager")]
    public virtual ICollection<Project> ManagedProjects { get; set; } = new List<Project>();
}
