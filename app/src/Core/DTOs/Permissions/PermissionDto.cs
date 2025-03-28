namespace zora.Core.DTOs.Permissions;

public sealed class PermissionDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PermissionString { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public required IEnumerable<long> RoleIds { get; set; }
    public required IEnumerable<long> WorkItemIds { get; set; }
}
