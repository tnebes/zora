#region

using zora.Core.DTOs.WorkItems;

#endregion

namespace zora.Core.DTOs.Assets;

public class AssetDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AssetPath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
}

public sealed class AssetWithWorkItemsDto : AssetDto
{
    public ICollection<WorkItemDto> WorkItems { get; set; } = new List<WorkItemDto>();
}
