namespace zora.Core.DTOs.Requests;

public sealed class DynamicQueryRoleParamsDto : DynamicQueryParamsDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? Permission { get; set; }
    public string? User { get; set; }
}
