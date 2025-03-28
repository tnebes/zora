namespace zora.Core.DTOs.Permissions;

public sealed class UpdatePermissionDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public string PermissionString { get; set; } = string.Empty;
    public IEnumerable<long> WorkItemIds { get; set; } = new List<long>();
}
