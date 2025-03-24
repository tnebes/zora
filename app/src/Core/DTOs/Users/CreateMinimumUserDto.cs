#region

using System.Text.Json.Serialization;

#endregion

namespace zora.Core.DTOs.Users;

public sealed class CreateMinimumUserDto : UserDto
{
    public required string Password { get; set; }
    public required string Email { get; set; }

    [JsonPropertyName("Roles")] public required IEnumerable<long> RoleIds { get; set; }
}
