#region

using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace zora.Core.Domain;

public class WorkItem : BaseEntity
{
    private IEnumerable<Asset>? _assets;
    public long Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public long? AssigneeId { get; set; }

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

    public virtual User? Assignee { get; set; }

    public virtual User? CreatedBy { get; set; }

    public virtual User? UpdatedBy { get; set; }

    public virtual ZoraProgram? Program { get; set; }

    public virtual Project? Project { get; set; }

    public virtual ZoraTask? Task { get; set; }

    public virtual ICollection<WorkItemRelationship> SourceRelationships { get; set; } =
        new List<WorkItemRelationship>();

    public virtual ICollection<WorkItemRelationship> TargetRelationships { get; set; } =
        new List<WorkItemRelationship>();

    public virtual ICollection<WorkItemAsset> WorkItemAssets { get; set; } = new List<WorkItemAsset>();

    public virtual ICollection<PermissionWorkItem> PermissionWorkItems { get; set; } = new List<PermissionWorkItem>();

    [NotMapped]
    public virtual IEnumerable<Asset> Assets
    {
        get => this._assets ??= this.WorkItemAssets.Select(wia => wia.Asset);
        set => this._assets = value;
    }
}
