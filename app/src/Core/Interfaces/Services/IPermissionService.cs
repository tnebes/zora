#region

using FluentResults;
using zora.Core.Domain;
using zora.Core.DTOs.Permissions;
using zora.Core.DTOs.Requests;

#endregion

namespace zora.Core.Interfaces.Services;

public interface
    IPermissionService : IBaseService<Permission, CreatePermissionDto, UpdatePermissionDto, PermissionResponseDto,
    DynamicQueryPermissionParamsDto>
{
    Task<bool> HasDirectPermissionAsync(PermissionRequestDto request);
    Task<Result<IEnumerable<Permission>>> GetAllAsync();
    Task<Result<PermissionResponseDto>> GetPermissionsByIdsAsync(List<long> ids);
}
