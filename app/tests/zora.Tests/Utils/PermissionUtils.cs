#region

using zora.Core.Domain;
using zora.Core.DTOs.Permissions;

#endregion

namespace zora.Tests.Utils;

public static class PermissionUtils
{
    public static IEnumerable<Permission> GetValidPermissions()
    {
        List<Permission> permissions =
        [
            new()
            {
                Id = 1,
                Name = "Create",
                Description = "Create permission",
                PermissionString = "00100",
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Id = 2,
                Name = "Read",
                Description = "Read permission",
                PermissionString = "00001",
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Id = 3,
                Name = "Update",
                Description = "Update permission",
                PermissionString = "00010",
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Id = 4,
                Name = "Delete",
                Description = "Delete permission",
                PermissionString = "01000",
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Id = 5,
                Name = "Admin",
                Description = "Admin permission",
                PermissionString = "10000",
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Id = 6,
                Name = "Super Admin",
                Description = "Super Admin permission",
                PermissionString = "11111",
                CreatedAt = DateTime.UtcNow
            }
        ];

        return permissions;
    }

    internal static UpdatePermissionDto GetValidUpdatePermissionDto()
    {
        return new UpdatePermissionDto
        {
            Name = "UpdatedPermission",
            Description = "Updated Description",
            PermissionString = "01010"
        };
    }
}
