#region

using FluentResults;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Requests.Interfaces;
using zora.Core.DTOs.Responses;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public sealed class AssetService : IAssetService, IZoraService
{

    private readonly AssetRepository _assetRepository;
    private readonly Logger<AssetService> _logger;

    public AssetService(AssetRepository assetRepository, Logger<AssetService> logger)
    {
        this._assetRepository = assetRepository;
        this._logger = logger;
    }

    public Task<Result<(IEnumerable<Asset>, int total)>> GetAsync(QueryParamsDto queryParams) =>
        throw new NotImplementedException();

    public Task<Result<AssetResponseDto>> GetDtoAsync(QueryParamsDto queryParams) =>
        throw new NotImplementedException();

    public Task<Result<Asset>> GetByIdAsync(long id) => throw new NotImplementedException();

    public Task<Result<Asset>> CreateAsync(CreateAssetDto createDto) => throw new NotImplementedException();

    public Task<Result<Asset>> UpdateAsync(long id, UpdateAssetDto updateDto) => throw new NotImplementedException();

    public Task<bool> DeleteAsync(long id) => throw new NotImplementedException();

    public Task<Result<AssetResponseDto>> FindAsync(QueryParamsDto findParams) => throw new NotImplementedException();

    public Task<Result<AssetResponseDto>> SearchAsync(DynamicQueryAssetParamsDto searchParams) =>
        throw new NotImplementedException();
    public Result<CreateAssetDto> IsValidAssetCreateDto(CreateAssetDto createDto) => throw new NotImplementedException();
    public Result<UpdateAssetDto> IsValidAssetUpdateDto(UpdateAssetDto updateDto) => throw new NotImplementedException();

    private string[] validateAssetDto(object dto)
    {
        throw new NotImplementedException();
    }
}
