namespace zora.Core.DTOs.Requests;

public sealed class CreateAssetDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string AssetPath { get; set; }
    public long WorkAssetId { get; set; }
    public IFormFile Asset { get; set; }
}
