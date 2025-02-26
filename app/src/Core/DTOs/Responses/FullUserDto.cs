namespace zora.Core.DTOs.Responses;

public sealed class FullUserDto : MinimumUserDto
{
    public required string Email { get; set; }
    public required DateTime CreatedAt { get; set; }

    public required Dictionary<long, string>
        Roles { get; set; } // TODO FIXME this should be a list of a specialised RoleDto
}
