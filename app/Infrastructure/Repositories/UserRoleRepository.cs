#region

using Microsoft.EntityFrameworkCore;
using zora.Core.Domain;
using zora.Core.Interfaces.Repositories;
using zora.Core.Interfaces.Services;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.Repositories;

public sealed class UserRoleRepository : BaseCompositeRepository<UserRole>, IUserRoleRepository, IZoraService
{
    private readonly ILogger<UserRoleRepository> _logger;

    public UserRoleRepository(
        ApplicationDbContext dbContext,
        ILogger<UserRoleRepository> logger) : base(dbContext, logger) =>
        this._logger = logger;

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

    public async Task<bool> AssignRoles(User user, List<long> roles)
    {
        try
        {
            long userId = user.Id;
            foreach (long roleId in roles)
            {
                UserRole userRole = new()
                {
                    UserId = userId,
                    RoleId = roleId
                };

                await this.CreateAsync(userRole);
            }

            return await Task.FromResult(true);
        }
        catch (Exception e)
        {
            this._logger.LogError(e, "Error assigning roles to user");
            return await Task.FromResult(false);
        }
    }

    public Task SaveChangesAsync() => this.DbContext.SaveChangesAsync();
}
