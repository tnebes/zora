#region

using zora.Core.DTOs.Responses.Interface;

#endregion

namespace zora.Core.DTOs.Roles;

public sealed class RoleResponseDto : IResponseDto<RoleDto>
{
    public required int Total { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public required IEnumerable<RoleDto> Items { get; set; }
}
