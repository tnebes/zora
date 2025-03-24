namespace zora.Core.DTOs.Roles;

public sealed class CreateRoleDto
{
    public required string Name { get; set; }
    public required IEnumerable<long> PermissionIds { get; set; }
}
