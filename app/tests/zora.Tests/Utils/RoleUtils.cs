#region

using zora.Core.Domain;

#endregion

namespace zora.Tests.Utils;

public static class RoleUtils
{
    public static IEnumerable<Role> GetValidRoles()
    {
        List<Role> roles =
        [
            new()
            {
                Id = 1,
                Name = "Admin",
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Id = 2,
                Name = "User",
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Id = 3,
                Name = "Manager",
                CreatedAt = DateTime.UtcNow
            }
        ];

        return roles;
    }

    public static IEnumerable<Role> GetFindTestRoles()
    {
        List<Role> roles =
        [
            new()
            {
                Id = 1,
                Name = "Admin Role",
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Id = 2,
                Name = "User Role",
                CreatedAt = DateTime.UtcNow
            },

            new()
            {
                Id = 3,
                Name = "Guest Role",
                CreatedAt = DateTime.UtcNow
            }
        ];

        return roles;
    }

    public static Role GetValidRole()
    {
        return new Role
        {
            Name = "Test Role"
        };
    }

    public static void AssignRoleToUser(long userId, long roleId)
    {
    }
}
