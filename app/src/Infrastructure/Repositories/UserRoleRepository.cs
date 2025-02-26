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

    public async Task<Result<IEnumerable<UserRole>>> GetByUserIdAsync(long userId, bool includeProperties = false)
    {
        try
        {
            IQueryable<UserRole> query = this.DbSet.AsQueryable();
            if (includeProperties)
            {
                query = this.IncludeProperties(query);
            }

            List<UserRole> userRoles = await query.Where(ur => ur.UserId == userId).ToListAsync();
            return Result.Ok(userRoles.AsEnumerable());
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting user roles for user {UserId}", userId);
            return Result.Fail<IEnumerable<UserRole>>("Failed to retrieve user roles");
        }
    }

    public async Task<Result<UserRole>> GetByCompositeKeyAsync(long userId, long roleId, bool includeProperties = false)
    {
        try
        {
            IQueryable<UserRole> query = this.DbSet.AsQueryable();
            if (includeProperties)
            {
                query = this.IncludeProperties(query);
            }

            UserRole? userRole = await query.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            return userRole == null
                ? Result.Fail<UserRole>("User role not found")
                : Result.Ok(userRole);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting user role for user {UserId} and role {RoleId}", userId, roleId);
            return Result.Fail<UserRole>("Failed to retrieve user role");
        }
    }

    public async Task<bool> ExistsAsync(long userId, long roleId)
    {
        try
        {
            return await this.FindByCondition(ur => ur.UserId == userId && ur.RoleId == roleId)
                .AnyAsync();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error checking if user role exists for user {UserId} and role {RoleId}", userId,
                roleId);
            return false;
        }
    }

    public new async Task<Result<UserRole>> CreateAsync(UserRole entity)
    {
        try
        {
            UserRole createdUserRole = await base.CreateAsync(entity);
            return createdUserRole == null
                ? Result.Fail<UserRole>("Failed to create user role")
                : Result.Ok(createdUserRole);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error creating user role");
            return Result.Fail<UserRole>("Failed to create user role");
        }
    }

    public async Task<bool> AssignRoles(User user, List<long> roles)
    {
        try
        {
            IExecutionStrategy strategy = this.DbContext.Database.CreateExecutionStrategy();
            bool result = await strategy.ExecuteAsync(async () =>
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

                        Result<UserRole> createResult = await this.CreateAsync(userRole);
                        if (createResult.IsFailed)
                        {
                            throw new Exception($"Failed to create user role for role {roleId}");
                        }
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
            return result;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error assigning roles to user {UserId}", user.Id);
            return false;
        }
    }

    private IQueryable<UserRole> IncludeProperties(IQueryable<UserRole> query)
    {
        return query.Include(ur => ur.Role)
            .Include(ur => ur.User);
    }
}
