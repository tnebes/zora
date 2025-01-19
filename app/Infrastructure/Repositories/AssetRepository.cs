#region

using FluentResults;
using Microsoft.EntityFrameworkCore;
using zora.Core;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.Interfaces.Services;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.Repositories;

public sealed class AssetRepository : BaseRepository<Asset>, IAssetRepository, IZoraService
{
    public AssetRepository(ApplicationDbContext dbContext, ILogger<AssetRepository> logger) : base(dbContext,
        logger)
    {
    }

    public async Task<Result<(IEnumerable<Asset>, int totalCount)>> SearchAsync(DynamicQueryAssetParamsDto searchParams,
        bool includeProperties = false)
    {
        try
        {
            IQueryable<Asset> query = this.DbSet.AsQueryable();

            if (includeProperties)
            {
                query = query.Include(a => a.WorkItemAssets);
            }

            query = this.GetQueryableAsset(searchParams, query);

            (IQueryable<Asset> filteredAssets, int totalCount) =
                await this.GetPagedAsync(query, searchParams.Page, searchParams.PageSize);

            return Result.Ok((filteredAssets.AsEnumerable(), totalCount));
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error searching assets. Exception: {ExceptionMessage}", Constants.ERROR_500_MESSAGE);
            return Result.Fail<(IEnumerable<Asset>, int totalCount)>(Constants.ERROR_500_MESSAGE);
        }
    }

    private IQueryable<Asset> GetQueryableAsset(DynamicQueryAssetParamsDto queryParams, IQueryable<Asset> query)
    {
        this.ApplyListFilter(ref query, queryParams.Id, long.Parse,
            (asset, ids) => ids.Contains(asset.Id));

        this.ApplyListFilter(ref query, queryParams.Name, s => s,
            (asset, names) => names.Contains(asset.Name));

        this.ApplyListFilter(ref query, queryParams.Description, s => s,
            (asset, descriptions) => descriptions.Contains(asset.Description));

        this.ApplyListFilter(ref query, queryParams.AssetPath, s => s,
            (asset, assetPaths) => assetPaths.Contains(asset.AssetPath));

        this.ApplyListFilter(ref query, queryParams.WorkItemId, long.Parse,
            (asset, workAssetIds) => asset.WorkItemAssets.Any(wi => workAssetIds.Contains(wi.WorkItemId)));

        return query;
    }
}
