namespace zora.Core.DTOs.Responses;

public class RoleDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public required IEnumerable<long> UserIds { get; set; }
    public required IEnumerable<long> PermissionIds { get; set; }
}
