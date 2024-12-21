#region

using AutoMapper;
using zora.Core.Domain;
using zora.Core.DTOs;
using zora.Core.DTOs.Responses;

#endregion

namespace zora.Infrastructure.Helpers;

public static class RoleDtoHelper
{
    public static RoleResponseDto ToRoleResponseDto(this IEnumerable<Role> roles, int total, int page,
        int pageSize,
        IMapper mapper)
    {
        return new RoleResponseDto
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            Items = roles.Select(mapper.Map<RoleDto>)
        };
    }
}
