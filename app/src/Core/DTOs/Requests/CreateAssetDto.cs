namespace zora.Core.DTOs.Requests;

public sealed class CreateAssetDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? AssetPath { get; set; }
    public long? WorkAssetId { get; set; }
    public long? CreatedById { get; set; }
    public required IFormFile Asset { get; set; }
}
