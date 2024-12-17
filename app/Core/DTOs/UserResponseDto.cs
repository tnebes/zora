#region

using zora.Core.DTOs.Interface;

#endregion

namespace zora.Core.DTOs;

public sealed class UserResponseDto<T> : IResponseDto<T> where T : UserDto
{
    public required IEnumerable<T> Items { get; set; }
    public required int Total { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
}
