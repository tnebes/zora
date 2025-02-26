#region

using System.ComponentModel.DataAnnotations;
using zora.Core.DTOs.Responses;

#endregion

namespace zora.Core.DTOs.Requests;

public sealed class UpdateUserDto : MinimumUserDto
{
    [EmailAddress] public required string Email { get; set; }

    public required IEnumerable<long> RoleIds { get; set; }
}
