#region

using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.Interfaces.Repositories;
using zora.Core.Interfaces.Services;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.Repositories;

public sealed class UserRepository : BaseRepository<User>, IUserRepository, IZoraService
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    public UserRepository(
        ApplicationDbContext dbContext,
        ILogger<UserRepository> logger) : base(dbContext, logger)
    {
    }

    public new async Task<Result<User>> GetByIdAsync(long id)
    {
        await UserRepository.Semaphore.WaitAsync();
        try
        {
            User? user = await base.GetByIdAsync(id);
            if (user == null)
            {
                return Result.Fail<User>(new Error("User not found"));
            }

            return Result.Ok(user);
        }
        finally
        {
            UserRepository.Semaphore.Release();
        }
    }

    public async Task<(IEnumerable<User>, int totalCount)> GetUsersAsync(QueryParamsDto queryParams) =>
        await this.GetPagedAsync(queryParams.Page, queryParams.PageSize);

    public async Task<bool> IsUsernameUniqueAsync(string username)
    {
        return !await this.FindByCondition(user => user.Username == username)
            .AnyAsync();
    }

    public async Task<bool> IsEmailUniqueAsync(string email)
    {
        return !await this.FindByCondition(user => user.Email == email)
            .AnyAsync();
    }

    public void Delete(User user) => this.DbSet.Remove(user);
    public async Task SaveChangesAsync() => await this.DbContext.SaveChangesAsync();

    public void SoftDelete(User user) => this.DbSet.Update(user);

    public async Task<User> Add(User user) => await this.AddAsync(user);

    public async Task<Result<User>> Update(User originalUser)
    {
        EntityEntry<User> entityEntry = this.DbSet.Update(originalUser);
        await this.SaveChangesAsync();
        return Result.Ok(entityEntry.Entity);
    }

    public async Task<Result<(IEnumerable<User> users, int totalCount)>> SearchUsers(IQueryable<User> query)
    {
        try
        {
            IEnumerable<User> users = await query.ToListAsync();
            int totalCount = await query.CountAsync();
            return Result.Ok((users, totalCount));
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to search users");
            return Result.Fail<(IEnumerable<User>, int)>(new Error("Failed to search users"));
        }
    }

    public async Task<Result<User>> GetByUsernameAsync(string username)
    {
        User? user = await this.FindByCondition(user => user.Username == username).FirstOrDefaultAsync();

        if (user == null)
        {
            return Result.Fail<User>(new Error("User not found"));
        }

        return Result.Ok(user);
    }

    public async Task<Result<User>> GetByEmailAsync(string email)
    {
        User? user = await this.FindByCondition(user => user.Email == email)
            .FirstOrDefaultAsync();

        return user == null ? Result.Fail<User>(new Error("User not found")) : Result.Ok(user);
    }

    public async Task<Result<(IEnumerable<User>, int totalCount)>> FindUsersAsync(QueryParamsDto findParams)
    {
        await UserRepository.Semaphore.WaitAsync();
        try
        {
            IQueryable<User> query = this.DbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u =>
                    EF.Functions.Like(u.Username, $"%{findParams.SearchTerm}%") ||
                    EF.Functions.Like(u.Email, $"%{findParams.SearchTerm}%") ||
                    u.UserRoles.Any(ur => EF.Functions.Like(ur.Role.Name, $"%{findParams.SearchTerm}%")));

            int totalCount = await query.CountAsync();

            List<User> users = await query
                .Skip((findParams.Page - 1) * findParams.PageSize)
                .Take(findParams.PageSize)
                .ToListAsync();

            return Result.Ok<(IEnumerable<User>, int)>((users, totalCount));
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to find users");
            return Result.Fail<(IEnumerable<User>, int)>(new Error("Failed to find users"));
        }
        finally
        {
            UserRepository.Semaphore.Release();
        }
    }
}
