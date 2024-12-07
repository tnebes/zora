#region

using zora.Core.Attributes;
using zora.Core.DTOs;
using zora.Core.Interfaces;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Singleton)]
public class PermissionRequestCacheService : IPermissionRequestCacheService, IZoraService
{
    private readonly Dictionary<string, PermissionRequestCachedDto> _cachedRequests;
    private readonly ILogger<PermissionRequestCacheService> _logger;
    private const int CACHE_EXPIRY_MINUTES = 5;

    public PermissionRequestCacheService(ILogger<PermissionRequestCacheService> logger)
    {
        this._logger = logger;
        this._cachedRequests = new Dictionary<string, PermissionRequestCachedDto>();
    }

    public PermissionRequestCachedDto GetCachedRequest(PermissionRequestDto permissionRequest)
    {
        if (permissionRequest == null)
        {
            throw new ArgumentNullException(nameof(permissionRequest));
        }

        string hash = permissionRequest.GetHashCode().ToString();
        if (this._cachedRequests.TryGetValue(hash, out PermissionRequestCachedDto cachedRequest))
        {
            this._logger.LogDebug("Permission request found in cache");
            DateTime now = DateTime.Now;
            if (cachedRequest.ExpiryDateTime > now)
            {
                this._logger.LogDebug("Permission request '{PermissionRequest}' stale", permissionRequest);
                this.RemoveCachedRequest(permissionRequest);
                return new PermissionRequestCachedDto
                {
                    IsExpired = true
                };
            }
        }

        this._logger.LogDebug("Permission request not found in cache");
        return new PermissionRequestCachedDto
        {
            DoesExist = false
        };
    }

    public void AddCachedRequest(PermissionRequestDto permissionRequest, bool isAuthorised)
    {
        if (permissionRequest == null)
        {
            this._logger.LogWarning("Permission request is null");
            throw new ArgumentNullException(nameof(permissionRequest));
        }

        string hash = permissionRequest.GetHashCode().ToString();
        if (!this._cachedRequests.ContainsKey(hash))
        {
            this._logger.LogDebug("Adding permission request {PermissionRequest} to cache", permissionRequest);
            this._cachedRequests.Add(hash, new PermissionRequestCachedDto
            {
                PermissionRequest = permissionRequest,
                IsAuthorised = isAuthorised,
                ExpiryDateTime = DateTime.Now.AddMinutes(CACHE_EXPIRY_MINUTES)
            });
        }
    }

    public void RemoveCachedRequest(PermissionRequestDto permissionRequest)
    {
        string hash = permissionRequest.GetHashCode().ToString();
        if (this._cachedRequests.ContainsKey(hash))
        {
            this._logger.LogDebug("Removing permission request {PermissionRequest} from cache", permissionRequest);
            this._cachedRequests.Remove(hash);
        }
    }

    public void ClearCache() => this._cachedRequests.Clear();
}
