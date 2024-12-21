#region

using System.Security.Claims;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;

#endregion

namespace zora.Core.Interfaces.Services;

public interface IRoleService
{
    bool IsRole(ClaimsPrincipal httpContextUser, string role);
    bool IsAdmin(ClaimsPrincipal httpContextUser);
    Task<bool> AssignRoles(User user, IEnumerable<long> roleIds);
    Task<(IEnumerable<Role>, int total)> GetRolesAsync(QueryParamsDto queryParams);
    Task<RoleResponseDto> GetRolesDtoAsync(QueryParamsDto queryParams);
}
