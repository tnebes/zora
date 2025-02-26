namespace zora.Core.DTOs.Requests;

public sealed class DynamicQueryUserParamsDto : DynamicQueryParamsDto
{
    public string? Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? Permission { get; set; }
}
