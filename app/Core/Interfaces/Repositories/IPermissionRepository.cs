#region

using FluentResults;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface IPermissionRepository
{
    Task<Result<IEnumerable<Permission>>> GetAllAsync();
    Task<Result<Permission>> GetByIdAsync(long id);
    Task<Result<Permission>> CreateAsync(Permission permission);
    Task<bool> DeleteAsync(Permission permission);
    Task<Result<(IEnumerable<Permission> permissions, int totalCount)>> GetPagedAsync(QueryParamsDto queryParams);
    Task<Result<(IEnumerable<Permission> permissions, int totalCount)>> FindPermissionsAsync(QueryParamsDto findParams);
    Task<Result<Permission>> UpdateAsync(Permission permission);
    Task<Result<(IEnumerable<Permission> permissions, int totalCount)>> GetByIdsAsync(List<long> ids);
}
