namespace zora.Core.Domain;

public abstract class WorkItem : BaseEntity
{
    protected WorkItem()
    {
        Type = GetType().Name;
    }

    public long Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public decimal? CompletionPercentage { get; set; }

    public decimal? EstimatedHours { get; set; }

    public decimal? ActualHours { get; set; }

    public DateTime CreatedAt { get; set; }

    public long? CreatedById { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public long? UpdatedById { get; set; }

    public long? AssigneeId { get; set; }

    public virtual User? Assignee { get; set; }

    public virtual User? CreatedBy { get; set; }

    public virtual User? UpdatedBy { get; set; }

    public virtual ICollection<WorkItemRelationship> SourceRelationships { get; set; } =
        new List<WorkItemRelationship>();

    public virtual ICollection<WorkItemRelationship> TargetRelationships { get; set; } =
        new List<WorkItemRelationship>();

    public virtual ICollection<WorkItemAsset> WorkItemAssets { get; set; } = new List<WorkItemAsset>();

    public virtual ICollection<PermissionWorkItem> PermissionWorkItems { get; set; } = new List<PermissionWorkItem>();
}
