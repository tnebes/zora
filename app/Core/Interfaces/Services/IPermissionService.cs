#region

using FluentResults;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;

#endregion

namespace zora.Core.Interfaces.Services;

public interface IPermissionService
{
    Task<bool> HasDirectPermissionAsync(PermissionRequestDto request);
    Task<Result<IEnumerable<Permission>>> GetAllAsync();
    Task<Result<Permission>> GetByIdAsync(long id);
    Task<Result<Permission>> CreateAsync(string name, string description, string permissionString);
    Task<bool> DeleteAsync(long id);
    Task<Result<PermissionResponseDto>> GetPermissionsDtoAsync(QueryParamsDto queryParams);
    Task<Result<PermissionResponseDto>> FindPermissionsAsync(QueryParamsDto findParams);
    Task<Result<Permission>> UpdateAsync(long id, string name, string description, string permissionString);
    Task<Result<PermissionResponseDto>> GetPermissionsByIdsAsync(List<long> ids);
}
