#region

using zora.Core.DTOs.Responses.Interface;

#endregion

namespace zora.Core.DTOs.Responses;

public sealed class RoleResponseDto : IResponseDto<RoleDto>
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public IEnumerable<RoleDto> Items { get; set; }
}
