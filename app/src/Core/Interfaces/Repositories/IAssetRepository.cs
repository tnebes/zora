#region

using FluentResults;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface IAssetRepository : ISearchRepository<Asset, DynamicQueryAssetParamsDto>
{
    Task<Result<Asset>> GetByIdAsync(long id, bool includeProperties = false);
    Task<Result<IEnumerable<Asset>>> GetAllAsync(bool includeProperties = false);

    Task<Result<(IEnumerable<Asset> Assets, int TotalCount)>> GetPagedAsync(QueryParamsDto paramsDto,
        bool includeProperties = false);

    Task<Result<Asset>> AddAsync(Asset entity);
    Task<Result<Asset>> UpdateAsync(Asset entity);
    Task<Result> DeleteAsync(long id);

    Task<Result<(IEnumerable<Asset> Assets, int TotalCount)>> FindByConditionAsync(string searchTerm,
        bool includeProperties = false);

    Task<Result<IEnumerable<Asset>>> GetQueryable(bool includeProperties = false);
    Task<Result<IEnumerable<Asset>>> FindAll(bool includeProperties = false);
}
