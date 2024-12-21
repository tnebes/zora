#region

using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.Enums;
using zora.Core.Interfaces.Repositories;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public sealed class PermissionService : IPermissionService, IZoraService
{
    private readonly ILogger<PermissionService> _logger;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IPermissionWorkItemRepository _permissionWorkItemRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IUserRoleService _userRoleService;

    public PermissionService(
        ILogger<PermissionService> logger,
        IRolePermissionRepository rolePermissionRepository,
        IPermissionRepository permissionRepository,
        IPermissionWorkItemRepository permissionWorkItemRepository,
        IUserRoleService userRoleService)
    {
        this._logger = logger;
        this._rolePermissionRepository = rolePermissionRepository;
        this._permissionRepository = permissionRepository;
        this._permissionWorkItemRepository = permissionWorkItemRepository;
        this._userRoleService = userRoleService;
    }

    public async Task<bool> HasDirectPermissionAsync(PermissionRequestDto request)
    {
        try
        {
            List<UserRole> userRoles =
                (await this._userRoleService.GetUserRolesByUserIdAsync(request.UserId)).ToList();

            if (this._userRoleService.IsAdminAsync(userRoles).Result)
            {
                this._logger.LogInformation(
                    "User {UserId} is an administrator and has all permissions",
                    request.UserId);
                return true;
            }

            foreach (UserRole userRole in userRoles)
            {
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

    // TODO does this account for C also giving R access?
    private static bool DoesPermissionGrantAccess(string permissionString, PermissionFlag requestedPermission)
    {
        int permissions = Convert.ToInt32(permissionString, 2);
        return (permissions & (int)requestedPermission) == (int)requestedPermission;
    }
}
