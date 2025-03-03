#region

using zora.Core.Domain;

#endregion

namespace zora.Tests.Utils;

public static class RelationshipUtils
{
    public static IEnumerable<UserRole> GetUserRoles()
    {
        List<UserRole> userRoles = new List<UserRole>
        {
            new()
            {
                UserId = 1,
                RoleId = 1
            },
            new()
            {
                UserId = 2,
                RoleId = 2
            },
            new()
            {
                UserId = 3,
                RoleId = 3
            }
        };

        return userRoles;
    }

    public static IEnumerable<RolePermission> GetRolePermissions()
    {
        List<RolePermission> rolePermissions = new List<RolePermission>
        {
            new()
            {
                RoleId = 1,
                PermissionId = 5
            },
            new()
            {
                RoleId = 2,
                PermissionId = 2
            },
            new()
            {
                RoleId = 3,
                PermissionId = 1
            },
            new()
            {
                RoleId = 3,
                PermissionId = 2
            },
            new()
            {
                RoleId = 3,
                PermissionId = 3
            }
        };

        return rolePermissions;
    }
}
