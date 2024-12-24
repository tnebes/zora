namespace zora.Core.DTOs.Responses;

public sealed class FullRoleDto : RoleDto
{
    public IDictionary<long, string> Users { get; set; } = new Dictionary<long, string>();
    public IDictionary<long, string> Permissions { get; set; } = new Dictionary<long, string>();
}
