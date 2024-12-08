#region

using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs;
using zora.Core.Enums;
using zora.Core.Interfaces;
using Constants = zora.Core.Constants;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public sealed class PermissionService : IPermissionService, IZoraService
{
    private readonly ILogger<PermissionService> _logger;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IPermissionWorkItemRepository _permissionWorkItemRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IUserRoleRepository _userRoleRepository;

    public PermissionService(
        ILogger<PermissionService> logger,
        IUserRoleRepository userRoleRepository,
        IRolePermissionRepository rolePermissionRepository,
        IPermissionRepository permissionRepository,
        IPermissionWorkItemRepository permissionWorkItemRepository)
    {
        this._logger = logger;
        this._userRoleRepository = userRoleRepository;
        this._rolePermissionRepository = rolePermissionRepository;
        this._permissionRepository = permissionRepository;
        this._permissionWorkItemRepository = permissionWorkItemRepository;
    }

    public async Task<bool> HasDirectPermissionAsync(PermissionRequestDto request)
    {
        try
        {
            IEnumerable<UserRole> userRoles = await this._userRoleRepository.GetByUserIdAsync(request.UserId);

            foreach (UserRole userRole in userRoles)
            {
                if (string.Equals(userRole.Role.Name, Constants.ADMIN, StringComparison.Ordinal))
                {
                    this._logger.LogInformation(
                        "User {UserId} is an administrator and has all permissions",
                        request.UserId);
                    return true;
                }

                IEnumerable<RolePermission> rolePermissions = await this._rolePermissionRepository
                    .GetByRoleIdAsync(userRole.RoleId);

                foreach (RolePermission rolePermission in rolePermissions)
                {
                    Permission? permission = await this._permissionRepository
                        .GetByIdAsync(rolePermission.PermissionId);

                    if (permission == null)
                    {
                        continue;
                    }

                    PermissionWorkItem? resourcePermission = await this._permissionWorkItemRepository
                        .GetByCompositeKeyAsync(permission.Id, request.ResourceId);

                    if (resourcePermission != null &&
                        PermissionService.DoesPermissionGrantAccess(permission.PermissionString,
                            request.RequestedPermission))
                    {
                        this._logger.LogInformation(
                            "Direct permission found for user {UserId} on resource {ResourceId}",
                            request.UserId, request.ResourceId);
                        return true;
                    }
                }
            }

            this._logger.LogInformation(
                "No direct permission found for user {UserId} on resource {ResourceId}",
                request.UserId, request.ResourceId);
            return false;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex,
                "Error checking direct permissions for user {UserId} on resource {ResourceId}",
                request.UserId, request.ResourceId);
            return false;
        }
    }

    private static bool DoesPermissionGrantAccess(string permissionString, PermissionFlag requestedPermission)
    {
        int permissions = Convert.ToInt32(permissionString, 2);
        return (permissions & (int)requestedPermission) == (int)requestedPermission;
    }
}
