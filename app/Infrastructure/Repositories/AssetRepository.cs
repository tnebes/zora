using FluentResults;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.Interfaces.Services;

namespace zora.Infrastructure.Repositories;

public sealed class AssetRepository : BaseRepository<Asset>, IAssetRepository, IZoraService
{
    

    public Task<Result<(IEnumerable<Asset>, int totalCount)>> SearchAsync(DynamicQueryAssetParamsDto searchParams, bool includeProperties = false) => throw new NotImplementedException();
}