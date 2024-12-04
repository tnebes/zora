namespace zora.Core.DTOs;

public class AssetDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AssetPath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? CreatedByUsername { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUsername { get; set; }
}

public class AssetWithWorkItemsDto : AssetDto
{
    public ICollection<WorkItemDto> WorkItems { get; set; } = new List<WorkItemDto>();
}
