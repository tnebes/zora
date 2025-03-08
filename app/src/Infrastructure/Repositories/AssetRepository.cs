#region

using FluentResults;
using Microsoft.EntityFrameworkCore;
using zora.Core;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.Interfaces.Repositories;
using zora.Core.Interfaces.Services;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.Repositories;

[ServiceLifetime(ServiceLifetime.Scoped)]
public sealed class AssetRepository : BaseRepository<Asset>, IAssetRepository, IZoraService
{
    public AssetRepository(ApplicationDbContext dbContext, ILogger<AssetRepository> logger) : base(dbContext,
        logger)
    {
    }

    public async Task<Result<(IEnumerable<Asset>, int TotalCount)>> SearchAsync(DynamicQueryAssetParamsDto searchParams,
        bool includeProperties = false)
    {
        try
        {
            IQueryable<Asset> query = this.BuildBaseQuery(includeProperties);
            query = this.GetQueryableAsset(searchParams, query);

            (IQueryable<Asset> filteredAssets, int totalCount) =
                await this.GetPagedAsync(query, searchParams.Page, searchParams.PageSize);
            List<Asset> assets = await filteredAssets.ToListAsync();
            return Result.Ok<(IEnumerable<Asset>, int)>((assets, totalCount));
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error searching assets");
            return Result.Fail<(IEnumerable<Asset>, int)>(Constants.ERROR_500_MESSAGE);
        }
    }

    public async Task<Result<Asset>> GetByIdAsync(long id, bool includeProperties = false)
    {
        try
        {
            IQueryable<Asset> query = this.BuildBaseQuery(includeProperties);
            Asset? asset = await query.FirstOrDefaultAsync(e => e.Id == id);

            return asset != null
                ? Result.Ok(asset)
                : Result.Fail<Asset>(Constants.ERROR_404_MESSAGE);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error getting asset by id {Id}", id);
            return Result.Fail<Asset>(Constants.ERROR_500_MESSAGE);
        }
    }

    public async Task<Result<IEnumerable<Asset>>> GetAllAsync(bool includeProperties = false)
    {
        try
        {
            IQueryable<Asset> query = this.BuildBaseQuery(includeProperties);
            List<Asset> assets = await query.ToListAsync();
            return Result.Ok<IEnumerable<Asset>>(assets);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error getting all assets");
            return Result.Fail<IEnumerable<Asset>>(Constants.ERROR_500_MESSAGE);
        }
    }

    public async Task<Result<(IEnumerable<Asset> Assets, int TotalCount)>> GetPagedAsync(QueryParamsDto paramsDto,
        bool includeProperties = false)
    {
        try
        {
            IQueryable<Asset> query = this.BuildBaseQuery(includeProperties);
            (IQueryable<Asset> filteredAssets, int totalCount) =
                await this.GetPagedAsync(query, paramsDto.Page, paramsDto.PageSize);
            List<Asset> assets = await filteredAssets.ToListAsync();
            return Result.Ok<(IEnumerable<Asset>, int)>((assets, totalCount));
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error getting paged assets");
            return Result.Fail<(IEnumerable<Asset>, int)>(Constants.ERROR_500_MESSAGE);
        }
    }

    public new async Task<Result<Asset>> AddAsync(Asset entity)
    {
        try
        {
            await this.DbSet.AddAsync(entity);
            await this.DbContext.SaveChangesAsync();
            return Result.Ok(entity);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error adding asset. Exception: {ExceptionMessage}", Constants.ERROR_500_MESSAGE);
            return Result.Fail<Asset>(Constants.ERROR_500_MESSAGE);
        }
    }

    public new async Task<Result<Asset>> UpdateAsync(Asset entity)
    {
        try
        {
            this.DbSet.Update(entity);
            await this.DbContext.SaveChangesAsync();
            return Result.Ok(entity);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error updating asset. Exception: {ExceptionMessage}",
                Constants.ERROR_500_MESSAGE);
            return Result.Fail(Constants.ERROR_500_MESSAGE);
        }
    }

    public new async Task<Result> DeleteAsync(long id)
    {
        try
        {
            Asset? asset = await this.FilteredDbSet.FirstOrDefaultAsync(a => a.Id == id);
            if (asset == null)
            {
                this.Logger.LogWarning("Asset with id {Id} not found", id);
                return Result.Fail(Constants.ERROR_404_MESSAGE);
            }

            asset.Deleted = true;
            this.DbSet.Update(asset);
            await this.DbContext.SaveChangesAsync();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error deleting asset. Exception: {ExceptionMessage}",
                Constants.ERROR_500_MESSAGE);
            return Result.Fail(Constants.ERROR_500_MESSAGE);
        }
    }

    public async Task<Result<(IEnumerable<Asset> Assets, int TotalCount)>> FindByConditionAsync(string searchTerm,
        bool includeProperties = false)
    {
        try
        {
            IQueryable<Asset> query = this.BuildBaseQuery(includeProperties);

            query = query.Where(a =>
                EF.Functions.Like(a.Name, $"%{searchTerm}%") ||
                EF.Functions.Like(a.Description, $"%{searchTerm}%") ||
                EF.Functions.Like(a.AssetPath, $"%{searchTerm}%") ||
                (a.CreatedBy != null && EF.Functions.Like(a.CreatedBy.Username, $"%{searchTerm}%")) ||
                (a.UpdatedBy != null && EF.Functions.Like(a.UpdatedBy.Username, $"%{searchTerm}%"))
            );

            List<Asset> assets = await query.ToListAsync();
            return Result.Ok<(IEnumerable<Asset>, int)>((assets, assets.Count));
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error finding assets by condition");
            return Result.Fail<(IEnumerable<Asset>, int)>(Constants.ERROR_500_MESSAGE);
        }
    }

    public async Task<Result<IEnumerable<Asset>>> GetQueryable(bool includeProperties = false)
    {
        try
        {
            IQueryable<Asset> query = this.BuildBaseQuery(includeProperties);
            List<Asset> assets = await query.ToListAsync();
            return Result.Ok<IEnumerable<Asset>>(assets);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error getting queryable assets");
            return Result.Fail<IEnumerable<Asset>>(Constants.ERROR_500_MESSAGE);
        }
    }

    public async Task<Result<IEnumerable<Asset>>> FindAll(bool includeProperties = false)
    {
        try
        {
            IQueryable<Asset> query = this.BuildBaseQuery(includeProperties);
            List<Asset> assets = await query.ToListAsync();
            return Result.Ok<IEnumerable<Asset>>(assets);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error finding all assets");
            return Result.Fail<IEnumerable<Asset>>(Constants.ERROR_500_MESSAGE);
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

    private IQueryable<Asset> BuildBaseQuery(bool includeProperties)
    {
        IQueryable<Asset> query = this.FilteredDbSet;
        return includeProperties
            ? query.Include(a => a.WorkItemAssets)
            : query;
    }
}
