#region

using zora.Core.DTOs;

#endregion

namespace zora.Core.Interfaces;

public interface IAssetRepository : IZoraService
{
    Task<AssetDto?> GetByIdAsync(long id);
    Task<AssetWithWorkItemsDto?> GetByIdWithWorkItemsAsync(long id);
    Task<IEnumerable<AssetDto>> GetAllAsync();
}
