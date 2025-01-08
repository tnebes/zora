#region

using FluentResults;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface IRoleRepository
{
    Task<IEnumerable<Role>> GetRolesByIdsAsync(IEnumerable<long> roleIds, bool includeProperties = false);
    Task<IEnumerable<Role>> GetRolesAsync(bool includeProperties = false);
    Task<(IEnumerable<Role>, int total)> GetPagedAsync(QueryParamsDto queryParams, bool includeProperties = false);
    Task<Result<Role>> CreateAsync(Role role);
    Task<Result<Role>> GetByIdAsync(long id, bool includeProperties = false);
    Task<Result<Role>> UpdateAsync(Role role);
    Task<bool> DeleteAsync(Role role);

    Task<Result<(IEnumerable<Role>, int totalCount)>> FindRolesAsync(QueryParamsDto findParams,
        bool includeProperties = false);

    Task<Result<(IEnumerable<Role> roles, int totalCount)>> SearchRoles(IQueryable<Role> query,
        bool includeProperties = false);
}
