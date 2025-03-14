#region

using zora.Core.DTOs.Responses.Interface;

#endregion

namespace zora.Core.DTOs.Permissions;

public sealed class PermissionResponseDto : IResponseDto<PermissionDto>
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public IEnumerable<PermissionDto> Items { get; set; }
}
