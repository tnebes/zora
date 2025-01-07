#region

using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
            .ToListAsync() ?? new List<UserRole>();
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

    public new async Task<Result<UserRole>> CreateAsync(UserRole entity)
    {
        UserRole createdUserRole = await base.CreateAsync(entity);
        if (createdUserRole == null)
        {
            this.Logger.LogError("Error creating user role {CreatedUserRole}", createdUserRole);
            return Result.Fail("Error creating user role");
        }

        return Result.Ok(createdUserRole);
    }

    public async Task<bool> AssignRoles(User user, List<long> roles)
    {
        try
        {
            IExecutionStrategy strategy = this.DbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using IDbContextTransaction transaction = await this.DbContext.Database.BeginTransactionAsync();
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

                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        catch (Exception e)
        {
            this.Logger.LogError(e, "Error assigning roles to user");
            return false;
        }
    }
}
