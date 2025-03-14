#region

using System.ComponentModel.DataAnnotations;
using zora.Core.Enums;

#endregion

namespace zora.Core.DTOs.Permissions;

public sealed class PermissionRequestDto
{
    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "UserId must be greater than 0")]
    public long UserId { get; set; }

    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "ResourceId must be greater than 0")]
    public long ResourceId { get; set; }

    [Required] public ResourceType ResourceType { get; set; }

    [Required] public PermissionFlag RequestedPermission { get; set; }
}
