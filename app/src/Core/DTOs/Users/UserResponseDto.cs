#region

using zora.Core.DTOs.Responses.Interface;

#endregion

namespace zora.Core.DTOs.Users;

public sealed class UserResponseDto<T> : IResponseDto<T> where T : MinimumUserDto
{
    public required int Total { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public required IEnumerable<T> Items { get; set; }
}
