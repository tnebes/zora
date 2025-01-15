using zora.Core.DTOs.Responses.Interface;

namespace zora.Core.DTOs.Responses;

public sealed class AssetResponseDto : IResponseDto<AssetDto>
{
    public required int Total { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public required IEnumerable<AssetDto> Items { get; set; }
}
