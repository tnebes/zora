namespace zora.Core.Domain;

public abstract class WorkItem : BaseEntity
{
    public long Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? AssigneeId { get; set; }
    public User? Assignee { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? CompletionPercentage { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? ActualHours { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedById { get; set; }
    public User? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedById { get; set; }
    public User? UpdatedBy { get; set; }
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    public ICollection<WorkItemRelationship> SourceRelationships { get; set; } = new List<WorkItemRelationship>();
    public ICollection<WorkItemRelationship> TargetRelationships { get; set; } = new List<WorkItemRelationship>();
}
