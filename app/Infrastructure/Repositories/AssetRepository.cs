#region

using zora.Core.Domain;
using zora.Core.DTOs;
using zora.Core.Interfaces;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.Repositories;

public class AssetRepository : BaseRepository<Asset>, IAssetRepository
{
    public AssetRepository(ApplicationDbContext dbContext, ILogger<BaseRepository<Asset>> logger) : base(dbContext,
        logger)
    {
    }

    public Task<AssetDto?> GetByIdAsync(long id) => throw new NotImplementedException();

    public Task<AssetWithWorkItemsDto?> GetByIdWithWorkItemsAsync(long id) => throw new NotImplementedException();

    public Task<IEnumerable<AssetDto>> GetAllAsync() => throw new NotImplementedException();
}
