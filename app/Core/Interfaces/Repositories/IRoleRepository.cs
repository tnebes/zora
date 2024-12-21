#region

using zora.Core.Domain;
using zora.Core.DTOs.Requests;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface IRoleRepository
{
    Task<IEnumerable<Role>> GetRolesByIdsAsync(IEnumerable<long> roleIds);
    Task<IEnumerable<Role>> GetRolesAsync();
    Task<(IEnumerable<Role>, int total)> GetPagedAsync(QueryParamsDto queryParams);
}
