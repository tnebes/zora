#region

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
}
