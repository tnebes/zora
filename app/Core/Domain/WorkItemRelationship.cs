#region

using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace zora.Core.Domain;

public class WorkItemRelationship : BaseEntity
{
    public long Id { get; set; }
    public long SourceItemId { get; set; }
    public long TargetItemId { get; set; }
    public string RelationshipType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    [NotMapped] public WorkItem SourceItem { get; set; } = null!;

    [NotMapped] public WorkItem TargetItem { get; set; } = null!;
}
