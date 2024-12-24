#region

using System.Security.Claims;
using FluentResults;
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
    Task<Result<(IEnumerable<Role>, int total)>> GetRolesAsync(QueryParamsDto queryParams);
    Task<Result<RoleResponseDto>> GetRolesDtoAsync(QueryParamsDto queryParams);
    Task<Result<Role>> CreateRoleAsync(CreateRoleDto roleDto);
    Task<Result<Role>> UpdateRoleAsync(long id, UpdateRoleDto roleDto);
    Task<bool> DeleteRoleAsync(long id);
    Task<Result<RoleResponseDto>> FindRolesAsync(QueryParamsDto findParams);
}
