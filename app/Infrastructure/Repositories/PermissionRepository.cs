#region

using Microsoft.EntityFrameworkCore;
using zora.Core.Domain;
using zora.Core.Interfaces.Repositories;
using zora.Core.Interfaces.Services;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.Repositories;

public sealed class PermissionRepository : BaseRepository<Permission>, IPermissionRepository, IZoraService
{
    public PermissionRepository(ApplicationDbContext dbContext, ILogger<PermissionRepository> logger) : base(
        dbContext, logger)
    {
    }

    public new async Task<Permission?> GetByIdAsync(long rolePermissionPermissionId) =>
        await this.DbSet.FirstOrDefaultAsync(permission => permission.Id == rolePermissionPermissionId);
}
