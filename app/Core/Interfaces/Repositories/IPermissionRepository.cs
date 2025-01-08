#region

using FluentResults;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface IPermissionRepository
{
    Task<Result<IEnumerable<Permission>>> GetAllAsync(bool includeProperties = false);
    Task<Result<Permission>> GetByIdAsync(long id, bool includeProperties = false);
    Task<Result<Permission>> CreateAsync(Permission permission);
    Task<bool> DeleteAsync(Permission permission);

    Task<Result<(IEnumerable<Permission> permissions, int totalCount)>> GetPagedAsync(QueryParamsDto queryParams,
        bool includeProperties = false);

    Task<Result<(IEnumerable<Permission> permissions, int totalCount)>> FindPermissionsAsync(QueryParamsDto findParams,
        bool includeProperties = false);

    Task<Result<Permission>> UpdateAsync(Permission permission);

    Task<Result<(IEnumerable<Permission> permissions, int totalCount)>> GetByIdsAsync(List<long> ids,
        bool includeProperties = false);
}
