#region

using AutoMapper;
using zora.Core.Domain;
using zora.Core.DTOs.Users;

#endregion

namespace zora.Infrastructure.Helpers;

public static class UserDtoHelper
{
    public static UserResponseDto<FullUserDto> ToFullUserResponseDto(this IEnumerable<User> users, int total, int page,
        int pageSize,
        IMapper mapper)
    {
        return new UserResponseDto<FullUserDto>
        {
            Items = users.Select(mapper.Map<FullUserDto>),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public static UserResponseDto<MinimumUserDto> ToUserResponseDto(this IEnumerable<User> users, int total, int page,
        int pageSize,
        IMapper mapper)
    {
        return new UserResponseDto<MinimumUserDto>
        {
            Items = users.Select(mapper.Map<MinimumUserDto>),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }
}
