using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.Interfaces.Repositories;

public interface IAssetRepository : ISearchRepository<Asset, DynamicQueryAssetParamsDto>
{
    
}