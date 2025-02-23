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

public sealed class PermissionRepository : BaseRepository<Permission>, IPermissionRepository, IZoraService
{
    public PermissionRepository(ApplicationDbContext dbContext, ILogger<PermissionRepository> logger)
        : base(dbContext, logger)
    {
    }

    public async Task<Result<IEnumerable<Permission>>> GetAllAsync(bool includeProperties = false)
    {
        try
        {
            IQueryable<Permission> query = this.FilteredDbSet.AsQueryable();
            if (includeProperties)
            {
                query = this.IncludeProperties(query);
            }

            List<Permission> permissions = await query.ToListAsync();

            return Result.Ok(permissions.AsEnumerable());
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error getting all permissions");
            return Result.Fail<IEnumerable<Permission>>("Failed to retrieve permissions");
        }
    }

    public async Task<Result<Permission>> GetByIdAsync(long id, bool includeProperties = false)
    {
        try
        {
            IQueryable<Permission> query = this.FilteredDbSet.AsQueryable();
            if (includeProperties)
            {
                query = this.IncludeProperties(query);
            }

            Permission? permission = await query.FirstOrDefaultAsync(p => p.Id == id);

            if (permission != null)
            {
                return Result.Ok(permission);
            }

            return Result.Fail<Permission>($"Permission with id {id} not found");
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error getting permission by id {Id}", id);
            return Result.Fail<Permission>($"Failed to retrieve permission with id {id}");
        }
    }

    public async Task<Result<Permission>> CreateAsync(Permission permission)
    {
        try
        {
            await this.AddAsync(permission);
            return Result.Ok(permission);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error creating permission");
            return Result.Fail<Permission>("Failed to create permission");
        }
    }

    public async Task<bool> DeleteAsync(Permission permission)
    {
        try
        {
            permission.Deleted = true;
            this.DbSet.Update(permission);
            await this.DbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error deleting permission with id {Id}", permission.Id);
            return false;
        }
    }

    public async Task<Result<(IEnumerable<Permission> permissions, int totalCount)>> GetPagedAsync(
        QueryParamsDto queryParams, bool includeProperties = false)
    {
        try
        {
            IQueryable<Permission> query = this.FilteredDbSet;

            int totalCount = await query.CountAsync();

            if (includeProperties)
            {
                query = query
                    .Include(p => p.RolePermissions)
                    .ThenInclude(rp => rp.Role)
                    .AsSplitQuery();
            }

            List<Permission> permissions = await query
                .OrderBy(p => p.Id)
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            return Result.Ok((permissions.AsEnumerable(), totalCount));
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error getting paged permissions");
            return Result.Fail<(IEnumerable<Permission>, int)>("Failed to retrieve paged permissions");
        }
    }

    public async Task<Result<(IEnumerable<Permission> permissions, int totalCount)>> FindPermissionsAsync(
        QueryParamsDto findParams, bool includeProperties = false)
    {
        try
        {
            IQueryable<Permission> query = this.FilteredDbSet.AsQueryable();
            if (includeProperties)
            {
                query = this.IncludeProperties(query);
            }

            query = query.Where(p => p.Name.Contains(findParams.SearchTerm) ||
                                     p.Description.Contains(findParams.SearchTerm));

            int totalCount = await query.CountAsync();

            List<Permission> permissions = await query
                .Skip((findParams.Page - 1) * findParams.PageSize)
                .Take(findParams.PageSize)
                .ToListAsync();

            return Result.Ok((permissions.AsEnumerable(), totalCount));
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error finding permissions with search term {SearchTerm}", findParams.SearchTerm);
            return Result.Fail<(IEnumerable<Permission>, int)>("Failed to find permissions");
        }
    }

    public new async Task<Result<Permission>> UpdateAsync(Permission permission)
    {
        try
        {
            await base.UpdateAsync(permission);
            return Result.Ok(permission);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error updating permission with id {Id}", permission.Id);
            return Result.Fail<Permission>("Failed to update permission");
        }
    }

    public async Task<Result<(IEnumerable<Permission> permissions, int totalCount)>> GetByIdsAsync(List<long> ids,
        bool includeProperties = false)
    {
        try
        {
            IQueryable<Permission> query = this.FilteredDbSet.AsQueryable();
            if (includeProperties)
            {
                query = this.IncludeProperties(query);
            }

            query = query.Where(p => ids.Contains(p.Id));

            int totalCount = await query.CountAsync();
            List<Permission> permissions = await query.ToListAsync();

            return Result.Ok((permissions.AsEnumerable(), totalCount));
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error getting permissions by ids");
            return Result.Fail<(IEnumerable<Permission>, int)>("Failed to retrieve permissions by ids");
        }
    }

    public async Task<Result<(IEnumerable<Permission>, int TotalCount)>> SearchAsync(
        DynamicQueryPermissionParamsDto searchParams, bool includeProperties = false)
    {
        IQueryable<Permission> query = this.FilteredDbSet.AsQueryable();
        query = this.GetQueryablePermission(searchParams, query);

        if (includeProperties)
        {
            query = this.IncludeProperties(query);
        }

        int totalCount = await query.CountAsync();
        List<Permission> permissions = await query.ToListAsync();

        return Result.Ok((permissions.AsEnumerable(), TotalCount: totalCount));
    }

    private IQueryable<Permission> IncludeProperties(IQueryable<Permission> query)
    {
        return query.Include(p => p.RolePermissions)
            .ThenInclude(rp => rp.Role);
    }

    private IQueryable<Permission> GetQueryablePermission(DynamicQueryPermissionParamsDto queryParams,
        IQueryable<Permission> query)
    {
        this.ApplyListFilter(ref query, queryParams.Id, long.Parse,
            (permission, ids) => ids.Contains(permission.Id));

        this.ApplyListFilter(ref query, queryParams.Name, s => s,
            (permission, names) => names.Contains(permission.Name));

        this.ApplyListFilter(ref query, queryParams.Description, s => s,
            (permission, descriptions) => descriptions.Contains(permission.Description));

        this.ApplyListFilter(ref query, queryParams.PermissionString, s => s,
            (permission, permissionStrings) => permissionStrings.Contains(permission.PermissionString));

        this.ApplyListFilter(ref query, queryParams.RoleIds, long.Parse,
            (permission, roles) => permission.RolePermissions.Any(rp => roles.Contains(rp.Role.Id)));

        this.ApplyListFilter(ref query, queryParams.WorkItemIds, long.Parse,
            (permission, workItems) => permission.PermissionWorkItems.Any(pw => workItems.Contains(pw.WorkItem.Id)));

        return query;
    }
}
