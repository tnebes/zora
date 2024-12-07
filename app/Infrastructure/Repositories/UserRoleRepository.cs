#region

using Microsoft.EntityFrameworkCore;
using zora.Core.Domain;
using zora.Core.Interfaces;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.Repositories;

public class UserRoleRepository : BaseCompositeRepository<UserRole>, IUserRoleRepository, IZoraService
{
    public UserRoleRepository(
        ApplicationDbContext dbContext,
        ILogger<UserRoleRepository> logger) : base(dbContext, logger)
    {
    }

    public async Task<IEnumerable<UserRole>> GetByUserIdAsync(long userId)
    {
        return await this.FindByCondition(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .ToListAsync();
    }

    public async Task<UserRole?> GetByCompositeKeyAsync(long userId, long roleId)
    {
        return await this.FindByCondition(ur =>
                ur.UserId == userId && ur.RoleId == roleId)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsAsync(long userId, long roleId)
    {
        return await this.FindByCondition(ur =>
                ur.UserId == userId && ur.RoleId == roleId)
            .AnyAsync();
    }
}
