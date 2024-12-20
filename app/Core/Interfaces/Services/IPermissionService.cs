#region

using zora.Core.DTOs.Requests;

#endregion

namespace zora.Core.Interfaces.Services;

public interface IPermissionService
{
    Task<bool> HasDirectPermissionAsync(PermissionRequestDto request);
}
