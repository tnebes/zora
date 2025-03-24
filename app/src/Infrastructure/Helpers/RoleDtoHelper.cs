#region

using AutoMapper;
using zora.Core.Domain;
using zora.Core.DTOs.Roles;

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
            Items = mapper.Map<IEnumerable<RoleDto>>(roles)
        };
    }
}
