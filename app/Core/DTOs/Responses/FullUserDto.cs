namespace zora.Core.DTOs.Responses;

public sealed class FullUserDto : UserDto
{
    public required string Email { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required IEnumerable<string> Roles { get; set; }
}
