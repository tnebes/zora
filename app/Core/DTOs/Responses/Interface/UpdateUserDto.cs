#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace zora.Core.DTOs.Responses.Interface;

public sealed class UpdateUserDto : MinimumUserDto
{
    [EmailAddress] public required string Email { get; set; }

    public required IEnumerable<long> RoleIds { get; set; }
}
