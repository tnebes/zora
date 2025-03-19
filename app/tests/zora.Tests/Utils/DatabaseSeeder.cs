#region

using Microsoft.EntityFrameworkCore;
using zora.Core.Domain;
using zora.Infrastructure.Data;

#endregion

namespace zora.Tests.Utils;

public static class DatabaseSeeder
{
    public static async Task SeedUsersAsync(ApplicationDbContext dbContext, IEnumerable<User> users)
    {
        foreach (User user in users)
        {
            if (!await dbContext.Users.AnyAsync(u => u.Id == user.Id))
            {
                await dbContext.Users.AddAsync(user);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    public static async Task SeedRolesAsync(ApplicationDbContext dbContext, IEnumerable<Role> roles)
    {
        foreach (Role role in roles)
        {
            if (!await dbContext.Roles.AnyAsync(r => r.Id == role.Id))
            {
                await dbContext.Roles.AddAsync(role);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    public static async Task SeedPermissionsAsync(ApplicationDbContext dbContext, IEnumerable<Permission> permissions)
    {
        foreach (Permission permission in permissions)
        {
            if (!await dbContext.Permissions.AnyAsync(p => p.Id == permission.Id))
            {
                await dbContext.Permissions.AddAsync(permission);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    public static async Task SeedUserRolesAsync(ApplicationDbContext dbContext, IEnumerable<UserRole> userRoles)
    {
        foreach (UserRole userRole in userRoles)
        {
            if (!await dbContext.UserRoles.AnyAsync(ur => ur.UserId == userRole.UserId && ur.RoleId == userRole.RoleId))
            {
                await dbContext.UserRoles.AddAsync(userRole);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    public static async Task SeedRolePermissionsAsync(ApplicationDbContext dbContext,
        IEnumerable<RolePermission> rolePermissions)
    {
        foreach (RolePermission rolePermission in rolePermissions)
        {
            if (!await dbContext.RolePermissions.AnyAsync(rp =>
                    rp.RoleId == rolePermission.RoleId && rp.PermissionId == rolePermission.PermissionId))
            {
                await dbContext.RolePermissions.AddAsync(rolePermission);
            }
        }

        await dbContext.SaveChangesAsync();
    }

}
