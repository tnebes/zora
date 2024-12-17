#region

using AutoMapper;
using zora.Core.Domain;
using zora.Core.DTOs;

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

    public static UserResponseDto<UserDto> ToUserResponseDto(this IEnumerable<User> users, int total, int page,
        int pageSize,
        IMapper mapper)
    {
        return new UserResponseDto<UserDto>
        {
            Items = users.Select(mapper.Map<UserDto>),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }
}
