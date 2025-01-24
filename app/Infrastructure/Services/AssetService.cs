#region

using AutoMapper;
using FluentResults;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;
using zora.Core.Interfaces.Repositories;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public sealed class AssetService : IAssetService, IZoraService
{
    private readonly IAssetRepository _assetRepository;
    private readonly ILogger<AssetService> _logger;
    private readonly IMapper _mapper;

    public AssetService(IAssetRepository assetRepository, ILogger<AssetService> logger, IMapper mapper)
    {
        this._assetRepository = assetRepository;
        this._logger = logger;
        this._mapper = mapper;
    }

    public async Task<Result<(IEnumerable<Asset>, int total)>> GetAsync(QueryParamsDto queryParams)
    {
        Result<(IEnumerable<Asset> Assets, int TotalCount)> assets = await this._assetRepository.GetPagedAsync(queryParams);
        return assets;
    }

    public async Task<Result<AssetResponseDto>> GetDtoAsync(QueryParamsDto queryParams)
    {
        Result<(IEnumerable<Asset> Assets, int TotalCount)> assetsResult = await this._assetRepository.GetPagedAsync(queryParams);

        if (assetsResult.IsFailed)
        {
            this._logger.LogError("Asset Service failed to receive assets. Errors: {Errors}", assetsResult.Errors);
            return Result.Fail<AssetResponseDto>(assetsResult.Errors);
        }

        (IEnumerable<Asset> assets, int totalCount) = assetsResult.Value;
        IEnumerable<AssetDto> assetDtos = this._mapper.Map<IEnumerable<AssetDto>>(assets);
        AssetResponseDto assetResponseDto = new AssetResponseDto
        {
            Total = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            Items = assetDtos
        };
        return Result.Ok(assetResponseDto);
    }

    public async Task<Result<Asset>> GetByIdAsync(long id)
    {
        return await this._assetRepository.GetByIdAsync(id);
    }

    public async Task<Result<Asset>> CreateAsync(CreateAssetDto createDto)
    {
        Asset asset = this._mapper.Map<Asset>(createDto);
        return await this._assetRepository.AddAsync(asset);
    }

    public async Task<Result<Asset>> UpdateAsync(long id, UpdateAssetDto updateDto)
    {
        Result<Asset> existingAssetResult = await this._assetRepository.GetByIdAsync(id);
        if (existingAssetResult.IsFailed)
        {
            this._logger.LogError("Failed to find asset with id {Id}. Errors: {Errors}", id, existingAssetResult.Errors);
            return existingAssetResult;
        }

        Asset existingAsset = existingAssetResult.Value;
        this._mapper.Map(updateDto, existingAsset);
        existingAsset.UpdatedAt = DateTime.UtcNow;

        return await this._assetRepository.UpdateAsync(existingAsset);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        Result<Asset> assetResult = await this._assetRepository.GetByIdAsync(id);
        if (assetResult.IsFailed)
        {
            this._logger.LogError("Failed to find asset with id {Id} for deletion. Errors: {Errors}", id, assetResult.Errors);
            return false;
        }

        Result deleteResult = await this._assetRepository.DeleteAsync(id);
        return deleteResult.IsSuccess;
    }

    public async Task<Result<AssetResponseDto>> FindAsync(QueryParamsDto findParams)
    {
        Result<(IEnumerable<Asset> Assets, int TotalCount)> assetsResult = await this._assetRepository.GetPagedAsync(findParams);
        if (assetsResult.IsFailed)
        {
            this._logger.LogError("Failed to find assets. Errors: {Errors}", assetsResult.Errors);
            return Result.Fail<AssetResponseDto>(assetsResult.Errors);
        }

        (IEnumerable<Asset> assets, int totalCount) = assetsResult.Value;
        IEnumerable<AssetDto> assetDtos = this._mapper.Map<IEnumerable<AssetDto>>(assets);
        AssetResponseDto response = new()
        {
            Total = totalCount,
            Page = findParams.Page,
            PageSize = findParams.PageSize,
            Items = assetDtos
        };

        return Result.Ok(response);
    }

    public async Task<Result<AssetResponseDto>> SearchAsync(DynamicQueryAssetParamsDto searchParams)
    {
        Result<(IEnumerable<Asset> Items, int Total)> searchResult = await this._assetRepository.SearchAsync(searchParams);
        if (searchResult.IsFailed)
        {
            this._logger.LogError("Failed to search assets. Errors: {Errors}", searchResult.Errors);
            return Result.Fail<AssetResponseDto>(searchResult.Errors);
        }

        (IEnumerable<Asset> assets, int totalCount) = searchResult.Value;
        IEnumerable<AssetDto> assetDtos = this._mapper.Map<IEnumerable<AssetDto>>(assets);
        AssetResponseDto response = new()
        {
            Total = totalCount,
            Page = searchParams.Page,
            PageSize = searchParams.PageSize,
            Items = assetDtos
        };

        return Result.Ok(response);
    }

    public Result<TRequestDto> ValidateDto<TRequestDto>(TRequestDto dto) where TRequestDto : class
    {
        if (dto == null)
        {
            return Result.Fail<TRequestDto>("Request DTO cannot be null");
        }

        if (dto is CreateAssetDto createAssetDto)
        {
            if (string.IsNullOrEmpty(createAssetDto.Name))
            {
                return Result.Fail<TRequestDto>("Name is required");
            }

            if (string.IsNullOrEmpty(createAssetDto.AssetPath))
            {
                return Result.Fail<TRequestDto>("Asset path is required");
            }

            if (createAssetDto.Asset == null)
            {
                return Result.Fail<TRequestDto>("Asset is required");
            }

            if (createAssetDto.WorkAssetId <= 0)
            {
                return Result.Fail<TRequestDto>("Work asset ID must be greater than 0");
            }
        }
        else if (dto is UpdateAssetDto updateAssetDto)
        {
            if (string.IsNullOrEmpty(updateAssetDto.Name))
            {
                return Result.Fail<TRequestDto>("Name is required");
            }

            if (updateAssetDto.File == null)
            {
                return Result.Fail<TRequestDto>("File is required");
            }
        }
        else 
        {
            this._logger.LogError("Invalid request DTO type: {DtoType}", dto.GetType().Name);
            throw new InvalidOperationException("Invalid request DTO type");
        }
        return Result.Ok(dto);
    }
}
