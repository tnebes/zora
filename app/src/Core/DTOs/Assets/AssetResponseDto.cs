#region

using zora.Core.DTOs.Responses.Interface;

#endregion

namespace zora.Core.DTOs.Assets;

public sealed class AssetResponseDto : IResponseDto<AssetDto>
{
    public required int Total { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public required IEnumerable<AssetDto> Items { get; set; }
}
