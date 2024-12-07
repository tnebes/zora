#region

using System.Security.Claims;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs;
using zora.Core.Enums;
using zora.Core.Interfaces;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public sealed class AuthorisationService : IAuthorisationService, IZoraService
{
    private readonly ILogger<AuthorisationService> _logger;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IPermissionRequestCacheService _permissionRequestCacheService;
    private readonly IPermissionWorkItemRepository _permissionWorkItemRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IWorkItemService _workItemService;

    public AuthorisationService(
        ILogger<AuthorisationService> logger,
        IUserRoleRepository userRoleRepository,
        IRolePermissionRepository rolePermissionRepository,
        IPermissionRepository permissionRepository,
        IPermissionWorkItemRepository permissionWorkItemRepository,
        IPermissionRequestCacheService permissionRequestCacheService,
        IWorkItemService workItemService)
    {
        this._logger = logger;
        this._userRoleRepository = userRoleRepository;
        this._rolePermissionRepository = rolePermissionRepository;
        this._permissionRepository = permissionRepository;
        this._permissionWorkItemRepository = permissionWorkItemRepository;
        this._permissionRequestCacheService = permissionRequestCacheService;
        this._workItemService = workItemService;
    }

    public async Task<bool> IsAuthorisedAsync(PermissionRequestDto? permissionRequest)
    {
        if (permissionRequest == null)
        {
            this._logger.LogWarning("Permission request is null");
            return false;
        }

        PermissionRequestCachedDto cachedRequest =
            this._permissionRequestCacheService.GetCachedRequest(permissionRequest);
        if (cachedRequest.DoesExist)
        {
            return cachedRequest.IsAuthorised;
        }

        try
        {
            IEnumerable<UserRole> userRoles = await this._userRoleRepository.GetByUserIdAsync(permissionRequest.UserId);

            List<RolePermission> rolePermissions = new();
            foreach (UserRole userRole in userRoles)
            {
                IEnumerable<RolePermission> userRolePermissions = await this._rolePermissionRepository
                    .GetByRoleIdAsync(userRole.RoleId);
                rolePermissions.AddRange(userRolePermissions);
            }

            List<Permission> permissions = new();
            foreach (RolePermission rolePermission in rolePermissions)
            {
                Permission? permission = await this._permissionRepository
                    .GetByIdAsync(rolePermission.PermissionId);
                if (permission != null)
                {
                    permissions.Add(permission);
                }
            }

            foreach (Permission permission in permissions)
            {
                PermissionWorkItem? resourcePermission = await this._permissionWorkItemRepository
                    .GetByCompositeKeyAsync(permission.Id, permissionRequest.ResourceId);

                if (resourcePermission != null)
                {
                    this._permissionRequestCacheService.AddCachedRequest(permissionRequest, true);

                    return this.DoesPermissionGrantAccess(
                        permission.PermissionString,
                        permissionRequest.RequestedPermission);
                }
            }

            this._logger.LogWarning("No permission found for user {UserId} on resource {ResourceId}",
                permissionRequest.UserId, permissionRequest.ResourceId);

            this._logger.LogInformation("Checking for permission on a higher-level resource.");

            await this.HasPermissionToAncestor(permissionRequest);

            this._permissionRequestCacheService.AddCachedRequest(permissionRequest, false);

            return false;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error checking authorisation for user {UserId} on resource {ResourceId}",
                permissionRequest.UserId, permissionRequest.ResourceId);
            return false;
        }
    }

    public ValidationResult ValidateRequestAndClaims(PermissionRequestDto? permissionRequest, ClaimsPrincipal user)
    {
        if (permissionRequest == null)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = "Permission request cannot be null",
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        string? userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = "User ID claim not found in token",
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }

        if (!long.TryParse(userIdClaim, out long tokenUserId))
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = "Invalid user ID in token",
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }

        if (tokenUserId != permissionRequest.UserId)
        {
            this._logger.LogWarning("User ID mismatch. Token ID: {TokenId}, Request ID: {RequestId}",
                tokenUserId, permissionRequest.UserId);
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = "User ID in request does not match authenticated user",
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        return new ValidationResult { IsValid = true };
    }

    private async Task<bool> HasPermissionToAncestor(PermissionRequestDto permissionRequest)
    {
        WorkItemType workItemType = await this._workItemService.GetWorkItemType(permissionRequest.ResourceId);
        WorkItem? workItem = await this._workItemService.GetNearestAncestorOf(workItemType, permissionRequest.ResourceId);

        if (workItem == null)
        {
            this._logger.LogWarning("No ancestor found for resource {ResourceId}", permissionRequest.ResourceId);
            return false;
        }

        PermissionRequestDto ancestorPermissionRequest = new PermissionRequestDto
        {
            UserId = permissionRequest.UserId,
            ResourceId = workItem.Id,
            RequestedPermission = permissionRequest.RequestedPermission
        };

        bool hasAncestorPermission = await this.IsAuthorisedAsync(ancestorPermissionRequest);
        if (hasAncestorPermission)
        {
            this._logger.LogInformation("Permission found on ancestor {AncestorId} for user {UserId} on resource {ResourceId}",
                workItem.Id, permissionRequest.UserId, permissionRequest.ResourceId);
            this._permissionRequestCacheService.AddCachedRequest(ancestorPermissionRequest, true);
            this._permissionRequestCacheService.AddCachedRequest(permissionRequest, true);
            return true;
        }
        this._logger.LogWarning("No permission found on ancestor {AncestorId} for user {UserId} on resource {ResourceId}",
            workItem.Id, permissionRequest.UserId, permissionRequest.ResourceId);
        this._permissionRequestCacheService.AddCachedRequest(ancestorPermissionRequest, false);
        this._permissionRequestCacheService.AddCachedRequest(permissionRequest, false);
        return false;
    }

    private bool DoesPermissionGrantAccess(string permissionString, PermissionFlag requestedPermission)
    {
        int permissions = Convert.ToInt32(permissionString, 2);
        return (permissions & (int)requestedPermission) == (int)requestedPermission;
    }
}
