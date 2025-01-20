#region

using zora.Core.Domain;
using zora.Core.DTOs.Requests;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface IAssetRepository : ISearchRepository<Asset, DynamicQueryAssetParamsDto>
{
}
