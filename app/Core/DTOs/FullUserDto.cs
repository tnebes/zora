#region

#endregion

namespace zora.Core.DTOs;

public sealed class FullUserDto : UserDto
{
    public required string Email { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required IEnumerable<string> Roles { get; set; }
}
