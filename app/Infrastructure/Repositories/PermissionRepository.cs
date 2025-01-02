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

    public new async Task<Result<IEnumerable<Permission>>> GetAllAsync()
    {
        try
        {
            List<Permission> permissions = await this.FindAll().ToListAsync();
            return Result.Ok(permissions.AsEnumerable());
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error getting all permissions");
            return Result.Fail<IEnumerable<Permission>>("Failed to retrieve permissions");
        }
    }

    public new async Task<Result<Permission>> GetByIdAsync(long id)
    {
        try
        {
            Permission? permission = await base.GetByIdAsync(id);

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
            await this.UpdateAsync(permission);
            return true;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error deleting permission with id {Id}", permission.Id);
            return false;
        }
    }

    public async Task<Result<(IEnumerable<Permission> permissions, int totalCount)>> GetPagedAsync(
        QueryParamsDto queryParams)
    {
        try
        {
            IQueryable<Permission> query = this.FindAll();

            int totalCount = await query.CountAsync();

            List<Permission> permissions = await query
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
        QueryParamsDto findParams)
    {
        try
        {
            IQueryable<Permission> query = this.FindAll()
                .Where(p => p.Name.Contains(findParams.SearchTerm) ||
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

    public async Task<Result<Permission>> UpdateAsync(Permission permission)
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

    public async Task<Result<(IEnumerable<Permission> permissions, int totalCount)>> GetByIdsAsync(List<long> ids)
    {
        try
        {
            IQueryable<Permission> query = this.FindAll()
                .Where(p => ids.Contains(p.Id));

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
}
