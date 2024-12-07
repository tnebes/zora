namespace zora.Core.DTOs;

public sealed class PermissionRequestCachedDto
{
    public PermissionRequestDto? PermissionRequest { get; set; }
    public bool IsAuthorised { get; set; } = false;
    public bool IsExpired { get; set; } = true;
    public DateTime? ExpiryDateTime { get; set; }
    public bool DoesExist { get; set; } = false;
}
