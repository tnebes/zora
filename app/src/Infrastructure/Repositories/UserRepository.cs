#region

using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.Interfaces.Repositories;
using zora.Core.Interfaces.Services;
using zora.Core.Utils;
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

    public async Task<Result<User>> GetByIdAsync(long id, bool includeProperties = false)
    {
        await Semaphore.WaitAsync();
        try
        {
            IQueryable<User> query = this.FilteredDbSet.AsQueryable();
            if (includeProperties)
            {
                query = this.IncludeProperties(query);
            }

            User? user = await query.FirstOrDefaultAsync(user => user.Id == id);
            return user == null ? Result.Fail<User>(new Error("User not found")) : Result.Ok(user);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error getting user by id {Id}", id);
            return Result.Fail<User>(new Error("Failed to get user by id"));
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task<(IEnumerable<User>, int totalCount)> GetUsersAsync(QueryParamsDto queryParams,
        bool includeProperties = false)
    {
        IQueryable<User> query = this.FilteredDbSet.AsQueryable();
        if (includeProperties)
        {
            query = this.IncludeProperties(query);
        }

        return await this.GetPagedAsync(query, queryParams.Page, queryParams.PageSize);
    }

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

    public async Task SoftDelete(User user)
    {
        this.DbSet.Update(user);
        await this.DbContext.SaveChangesAsync();
    }

    public async Task<User> Add(User user) => await this.AddAsync(user);

    public async Task<Result<User>> Update(User originalUser)
    {
        EntityEntry<User> entityEntry = this.DbSet.Update(originalUser);
        await this.DbContext.SaveChangesAsync();
        return Result.Ok(entityEntry.Entity);
    }

    public async Task<Result<(IEnumerable<User> users, int TotalCount)>> SearchUsersAsync(IQueryable<User> query,
        bool includeProperties = false)
    {
        try
        {
            if (includeProperties)
            {
                query = this.IncludeProperties(query);
            }

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

    public async Task<Result<User>> GetByUsernameAsync(string username, bool includeProperties = false)
    {
        IQueryable<User> query = this.FilteredDbSet.AsQueryable();
        if (includeProperties)
        {
            query = this.IncludeProperties(query);
        }

        User? user = await query.FirstOrDefaultAsync(user => user.Username == username);

        if (user == null)
        {
            return Result.Fail<User>(new Error("User not found"));
        }

        return Result.Ok(user);
    }

    public async Task<Result<User>> GetByEmailAsync(string email, bool includeProperties = false)
    {
        User? user = await this.FindByCondition(user => user.Email == email)
            .FirstOrDefaultAsync();

        return user == null ? Result.Fail<User>(new Error("User not found")) : Result.Ok(user);
    }

    public async Task<Result<(IEnumerable<User>, int totalCount)>> FindUsersAsync(QueryParamsDto findParams,
        bool includeProperties = false)
    {
        await Semaphore.WaitAsync();
        try
        {
            IQueryable<User> query = this.FilteredDbSet.AsQueryable();
            if (includeProperties)
            {
                query = this.IncludeProperties(query);
            }

            query = query.Where(u =>
                EF.Functions.Like(u.Username, $"%{findParams.SearchTerm}%") ||
                EF.Functions.Like(u.Email, $"%{findParams.SearchTerm}%") ||
                u.UserRoles.Any(ur => EF.Functions.Like(ur.Role.Name, $"%{findParams.SearchTerm}%")));

            int totalCount = await query.CountAsync();

            int totalPages = PaginationUtils.CalculateTotalPages(totalCount, findParams.PageSize);
            int page = PaginationUtils.AdjustPage(findParams.Page, totalPages);

            List<User> users = await query
                .Skip(PaginationUtils.CalculateSkip(page, findParams.PageSize))
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
            Semaphore.Release();
        }
    }

    public async Task<Result<(IEnumerable<User>, int TotalCount)>> SearchAsync(DynamicQueryUserParamsDto searchParams,
        bool includeProperties = false)
    {
        IQueryable<User> query = this.FilteredDbSet.AsQueryable();
        query = this.GetQueryableUser(searchParams, query);
        return await this.SearchUsersAsync(query, includeProperties);
    }

    private IQueryable<User> IncludeProperties(IQueryable<User> query)
    {
        return query.Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(ur => ur.RolePermissions)
            .ThenInclude(ur => ur.Permission)
            .Include(u => u.AssignedWorkItems)
            .Include(u => u.CreatedWorkItems)
            .Include(u => u.UpdatedWorkItems)
            .Include(u => u.ManagedProjects);
    }

    private IQueryable<User> GetQueryableUser(DynamicQueryUserParamsDto queryParams, IQueryable<User> query)
    {
        this.ApplyListFilter(ref query, queryParams.Id, long.Parse,
            (user, ids) => ids.Contains(user.Id));

        this.ApplyListFilter(ref query, queryParams.Username, s => s,
            (user, usernames) => usernames.Contains(user.Username));

        this.ApplyListFilter(ref query, queryParams.Email, s => s,
            (user, emails) => emails.Contains(user.Email));

        this.ApplyListFilter(ref query, queryParams.Role, long.Parse,
            (user, roles) => user.UserRoles.Any(ur => roles.Contains(ur.Role.Id)));

        this.ApplyListFilter(ref query, queryParams.Permission, long.Parse,
            (user, permissions) => user.UserRoles.Any(ur =>
                ur.Role.RolePermissions.Any(rp => permissions.Contains(rp.Permission.Id))));

        this.ApplyFilter(ref query, queryParams.CreatedAt,
            (user, date) => date.Equals(user.CreatedAt));

        return query;
    }
}
