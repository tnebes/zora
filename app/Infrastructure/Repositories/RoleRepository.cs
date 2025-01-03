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

    public async Task<IEnumerable<Role>> GetRolesByIdsAsync(IEnumerable<long> roleIds)
    {
        return await this._dbContext.Roles
            .Where(r => roleIds.Contains(r.Id))
            .ToListAsync();
    }

    public async Task<IEnumerable<Role>> GetRolesAsync() => await this.GetAllAsync().ToListAsync();

    public async Task<(IEnumerable<Role>, int total)> GetPagedAsync(QueryParamsDto queryParams) =>
        await base.GetPagedAsync(queryParams.Page, queryParams.PageSize);

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
            return Result.Fail<Role>(e.Message);
        }
    }

    public new async Task<Result<Role>> GetByIdAsync(long id)
    {
        try
        {
            Role? role = await base.GetByIdAsync(id);
            return role != null
                ? Result.Ok(role)
                : Result.Fail<Role>($"Role with id {id} not found");
        }
        catch (Exception e)
        {
            this._logger.LogError(e, "Failed to get role by id: {ErrorMessage}", e.Message);
            return Result.Fail<Role>(e.Message);
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
            return Result.Fail<Role>(e.Message);
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

    public async Task<Result<(IEnumerable<Role>, int totalCount)>> FindRolesAsync(QueryParamsDto findParams)
    {
        try
        {
            IQueryable<Role> query = this.DbContext.Roles
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .Where(r =>
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
}
