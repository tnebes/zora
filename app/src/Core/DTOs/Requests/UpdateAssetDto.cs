namespace zora.Core.DTOs.Requests;

public sealed class UpdateAssetDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public long WorkItemId { get; set; }
    public long UpdatedById { get; set; }
    public IFormFile? File { get; set; }
}
