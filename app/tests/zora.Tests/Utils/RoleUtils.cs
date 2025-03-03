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
}
