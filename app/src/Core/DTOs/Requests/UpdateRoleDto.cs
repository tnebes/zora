namespace zora.Core.DTOs.Requests;

public sealed class UpdateRoleDto
{
    public required string Name { get; set; }

    public required IEnumerable<long> PermissionIds { get; set; }
}
