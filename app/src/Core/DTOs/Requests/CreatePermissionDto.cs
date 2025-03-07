namespace zora.Core.DTOs.Requests;

public sealed class CreatePermissionDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public string PermissionString { get; set; } = string.Empty;
}
