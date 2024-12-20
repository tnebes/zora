#region

using zora.Core.Domain;
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

    public IQueryable<Role> GetRoles(IEnumerable<long> roleIds) =>
        this._dbContext.Roles.Where(r => roleIds.Contains(r.Id));
}
