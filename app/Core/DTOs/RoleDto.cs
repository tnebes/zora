namespace zora.Core.DTOs;

public sealed class RoleDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public IEnumerable<long> UserIds { get; set; }
    public IEnumerable<long> PermissionIds { get; set; }
}
