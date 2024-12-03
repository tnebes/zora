namespace zora.Core.Domain;

public class Asset : BaseEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AssetPath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long? CreatedById { get; set; }
    public User? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedById { get; set; }
    public User? UpdatedBy { get; set; }
    public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}
