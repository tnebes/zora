#region

using FluentResults;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public sealed class AssetService : IAssetService, IZoraService
{
    private readonly IAssetRepository _assetRepository;
    private readonly ILogger<AssetService> _logger;

    public AssetService(IAssetRepository assetRepository, ILogger<AssetService> logger)
    {
        this._assetRepository = assetRepository;
        this._logger = logger;
    }

    public async Task<Result<(IEnumerable<Asset>, int total)>> GetAsync(QueryParamsDto queryParams)
    {
        var assets = await this._assetRepository.GetPagedAsync(queryParams);
    }

    public Task<Result<AssetResponseDto>> GetDtoAsync(QueryParamsDto queryParams) =>
        throw new NotImplementedException();

    public Task<Result<Asset>> GetByIdAsync(long id) => throw new NotImplementedException();

    public Task<Result<Asset>> CreateAsync(CreateAssetDto createDto) => throw new NotImplementedException();

    public Task<Result<Asset>> UpdateAsync(long id, UpdateAssetDto updateDto) => throw new NotImplementedException();

    public Task<bool> DeleteAsync(long id) => throw new NotImplementedException();

    public Task<Result<AssetResponseDto>> FindAsync(QueryParamsDto findParams) => throw new NotImplementedException();

    public Task<Result<AssetResponseDto>> SearchAsync(DynamicQueryAssetParamsDto searchParams) =>
        throw new NotImplementedException();

    public Result<TRequestDto> ValidateDto<TRequestDto>(TRequestDto dto) where TRequestDto : class =>
        throw new NotImplementedException();
}
