#region

using FluentResults;
using Microsoft.EntityFrameworkCore;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.Interfaces.Repositories;
using zora.Core.Interfaces.Services;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.Repositories;

public sealed class RoleRepository : BaseRepository<Role>, IRoleRepository, IZoraService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<RoleRepository> _logger;

    public RoleRepository(ApplicationDbContext dbContext, ILogger<RoleRepository> logger) : base(dbContext, logger)
    {
        this._logger = logger;
        this._dbContext = dbContext;
    }

    public async Task<IEnumerable<Role>> GetRolesByIdsAsync(IEnumerable<long> roleIds, bool includeProperties = false)
    {
        IQueryable<Role> query = this.FilteredDbSet.AsQueryable();
        if (includeProperties)
        {
            query = this.IncludeProperties(query);
        }

        return await query
            .Where(r => roleIds.Contains(r.Id))
            .ToListAsync();
    }

    public async Task<IEnumerable<Role>> GetRolesAsync(bool includeProperties = false)
    {
        IQueryable<Role> query = this.FilteredDbSet.AsQueryable();
        if (includeProperties)
        {
            query = this.IncludeProperties(query);
        }

        return await query.ToListAsync();
    }

    public async Task<(IEnumerable<Role>, int total)> GetPagedAsync(QueryParamsDto queryParams,
        bool includeProperties = false)
    {
        IQueryable<Role> query = this.FilteredDbSet.AsQueryable();
        if (includeProperties)
        {
            query = this.IncludeProperties(query);
        }

        return await base.GetPagedAsync(query, queryParams.Page, queryParams.PageSize);
    }

    public async Task<Result<Role>> CreateAsync(Role role)
    {
        try
        {
            await this._dbContext.Roles.AddAsync(role);
            await this._dbContext.SaveChangesAsync();
            return Result.Ok(role);
        }
        catch (Exception e)
        {
            this._logger.LogError(e, "Failed to create role: {ErrorMessage}", e.Message);
            return Result.Fail<Role>(new Error("Failed to create role").CausedBy(e));
        }
    }

    public new async Task<Result<Role>> GetByIdAsync(long id, bool includeProperties = false)
    {
        try
        {
            IQueryable<Role> query = this.FilteredDbSet.AsQueryable();
            if (includeProperties)
            {
                query = this.IncludeProperties(query);
            }

            Role? role = await query.FirstOrDefaultAsync(r => r.Id == id);
            return role != null
                ? Result.Ok(role)
                : Result.Fail<Role>($"Role with id {id} not found");
        }
        catch (Exception e)
        {
            this._logger.LogError(e, "Failed to get role by id: {ErrorMessage}", e.Message);
            return Result.Fail<Role>(new Error("Failed to get role by id").CausedBy(e));
        }
    }

    public new async Task<Result<Role>> UpdateAsync(Role role)
    {
        try
        {
            await base.UpdateAsync(role);
            return Result.Ok(role);
        }
        catch (Exception e)
        {
            this._logger.LogError(e, "Failed to update role: {ErrorMessage}", e.Message);
            return Result.Fail<Role>(new Error("Failed to update role").CausedBy(e));
        }
    }

    public async Task<bool> DeleteAsync(Role role)
    {
        try
        {
            await this.UpdateAsync(role);
            return true;
        }
        catch (Exception e)
        {
            this._logger.LogError(e, "Error during role deletion: {ErrorMessage}", e.Message);
            return false;
        }
    }

    public async Task<Result<(IEnumerable<Role>, int totalCount)>> FindRolesAsync(QueryParamsDto findParams,
        bool includeProperties = false)
    {
        try
        {
            IQueryable<Role> query = this.FilteredDbSet.AsQueryable();
            if (includeProperties)
            {
                query = this.IncludeProperties(query);
            }

            query = query.Where(r =>
                EF.Functions.Like(r.Name, $"%{findParams.SearchTerm}%"));

            int totalCount = await query.CountAsync();

            List<Role> roles = await query
                .Skip((findParams.Page - 1) * findParams.PageSize)
                .Take(findParams.PageSize)
                .ToListAsync();

            return Result.Ok<(IEnumerable<Role>, int)>((roles, totalCount));
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to find roles");
            return Result.Fail<(IEnumerable<Role>, int)>(new Error("Failed to find roles"));
        }
    }

    public async Task<Result<(IEnumerable<Role>, int totalCount)>> SearchAsync(DynamicQueryRoleParamsDto searchParams,
        bool includeProperties = false)
    {
        IQueryable<Role> query = this.FilteredDbSet.AsQueryable();
        query = this.GetQueryableRole(searchParams, query);
        return await this.SearchRolesAsync(query, includeProperties);
    }

    public async Task<Result<(IEnumerable<Role> roles, int totalCount)>> SearchRolesAsync(IQueryable<Role> query,
        bool includeProperties = false)
    {
        try
        {
            if (includeProperties)
            {
                query = this.IncludeProperties(query);
            }

            IEnumerable<Role> roles = await query.ToListAsync();
            int totalCount = await query.CountAsync();
            return Result.Ok((roles, totalCount));
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to search roles");
            return Result.Fail<(IEnumerable<Role>, int)>(new Error("Failed to search roles"));
        }
    }

    private IQueryable<Role> IncludeProperties(IQueryable<Role> query)
    {
        return query.Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .Include(r => r.UserRoles)
            .ThenInclude(ur => ur.User)
            .Include(r => r.UserRoles);
    }

    private IQueryable<Role> GetQueryableRole(DynamicQueryRoleParamsDto queryParams, IQueryable<Role> query)
    {
        this.ApplyListFilter(ref query, queryParams.Id, long.Parse,
            (role, ids) => ids.Contains(role.Id));

        this.ApplyListFilter(ref query, queryParams.Name, s => s,
            (role, names) => names.Contains(role.Name));

        this.ApplyListFilter(ref query, queryParams.Permission, long.Parse,
            (role, permissions) => role.RolePermissions.Any(rp => permissions.Contains(rp.Permission.Id)));

        this.ApplyListFilter(ref query, queryParams.User, long.Parse,
            (role, users) => role.UserRoles.Any(ur => users.Contains(ur.User.Id)));

        this.ApplyFilter(ref query, queryParams.CreatedAt,
            (role, date) => role.CreatedAt == date);

        return query;
    }
}
