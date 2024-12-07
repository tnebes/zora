using zora.Core.DTOs;

namespace zora.Core.Interfaces;

public interface IPermissionService
{
    Task<bool> HasDirectPermissionAsync(PermissionRequestDto request);
}
