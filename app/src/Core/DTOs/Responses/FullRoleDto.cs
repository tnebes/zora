namespace zora.Core.DTOs.Responses;

public sealed class FullRoleDto : RoleDto
{
    public IDictionary<long, string> Users { get; init; } = new Dictionary<long, string>();
    public IDictionary<long, string> Permissions { get; init; } = new Dictionary<long, string>();
}
