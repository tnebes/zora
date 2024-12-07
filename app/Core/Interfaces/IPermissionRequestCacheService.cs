using zora.Core.DTOs;

namespace zora.Core.Interfaces;

public interface IPermissionRequestCacheService
{
    PermissionRequestCachedDto GetCachedRequest(PermissionRequestDto permissionRequest);
    void AddCachedRequest(PermissionRequestDto permissionRequest, bool isAuthorised);
    void RemoveCachedRequest(PermissionRequestDto permissionRequest);
    void ClearCache();
}
