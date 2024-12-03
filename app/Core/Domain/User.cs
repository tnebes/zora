#region

using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace zora.Core.Domain;

public class User : BaseEntity
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    [NotMapped] public virtual ICollection<WorkItem> AssignedWorkItems { get; set; } = new List<WorkItem>();

    [NotMapped] public virtual ICollection<WorkItem> CreatedWorkItems { get; set; } = new List<WorkItem>();

    [NotMapped] public virtual ICollection<WorkItem> UpdatedWorkItems { get; set; } = new List<WorkItem>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
