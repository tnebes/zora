#region

using AutoMapper;
using FluentResults;
using zora.Core;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs.Assets;
using zora.Core.DTOs.Requests;
using zora.Core.Interfaces.Repositories;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public sealed class AssetService : IAssetService, IZoraService
{
    private readonly IAssetPathService _assetPathService;
    private readonly IAssetRepository _assetRepository;
    private readonly ILogger<AssetService> _logger;
    private readonly IMapper _mapper;

    public AssetService(
        IAssetRepository assetRepository,
        ILogger<AssetService> logger,
        IMapper mapper,
        IAssetPathService assetPathService)
    {
        this._assetRepository = assetRepository;
        this._logger = logger;
        this._mapper = mapper;
        this._assetPathService = assetPathService;
    }

    public async Task<Result<(IEnumerable<Asset>, int total)>> GetAsync(QueryParamsDto queryParams)
    {
        Result<(IEnumerable<Asset> Assets, int TotalCount)> assets =
            await this._assetRepository.GetPagedAsync(queryParams);
        return assets;
    }

    public async Task<Result<AssetResponseDto>> GetDtoAsync(QueryParamsDto queryParams)
    {
        Result<(IEnumerable<Asset> Assets, int TotalCount)> assetsResult =
            await this._assetRepository.GetPagedAsync(queryParams);

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

    public async Task<Result<Asset>> GetByIdAsync(long id, bool includeProperties = false) =>
        await this._assetRepository.GetByIdAsync(id, includeProperties);

    public async Task<Result<Asset>> CreateAsync(CreateAssetDto createDto, long userId)
    {
        createDto.CreatedById = userId;
        return await this.CreateAsync(createDto);
    }

    public async Task<Result<Asset>> CreateAsync(CreateAssetDto createDto)
    {
        try
        {
            string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(createDto.Asset.FileName)}";
            string uploadDirectory = this._assetPathService.GetAssetsBasePath();
            Directory.CreateDirectory(uploadDirectory);

            string filePath = Path.Combine(uploadDirectory, fileName);

            await using FileStream stream = new FileStream(filePath, FileMode.Create);
            await createDto.Asset.CopyToAsync(stream);
            this._logger.LogDebug("Asset file saved to {FilePath}", filePath);

            createDto.AssetPath = this._assetPathService.GetAssetWebPath(fileName);
            Asset asset = this._mapper.Map<Asset>(createDto);

            return await this._assetRepository.AddAsync(asset);
        }
        catch (IOException ex)
        {
            this._logger.LogError(ex, "File system error creating asset: {Message}", ex.Message);
            return Result.Fail<Asset>("File system error while saving asset");
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Unexpected error creating asset: {Message}", ex.Message);
            return Result.Fail<Asset>("Unexpected error while creating asset");
        }
    }

    public async Task<Result<Asset>> UpdateAsync(long id, UpdateAssetDto updateDto)
    {
        try
        {
            Result<Asset> existingAsset = await this.GetByIdAsync(id);
            if (existingAsset.IsFailed)
            {
                return existingAsset;
            }

            if (updateDto == null)
            {
                return Result.Fail<Asset>("Update data is required");
            }

            Asset asset = existingAsset.Value;

            if (updateDto.File is { Length: > 0 })
            {
                string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(updateDto.File.FileName)}";
                string uploadDirectory = this._assetPathService.GetAssetsBasePath();
                string filePath = Path.Combine(uploadDirectory, fileName);

                await using FileStream stream = new FileStream(filePath, FileMode.Create);
                await updateDto.File.CopyToAsync(stream);

                asset.AssetPath = this._assetPathService.GetAssetWebPath(fileName);
            }

            this._mapper.Map(updateDto, asset);

            return await this._assetRepository.UpdateAsync(asset);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error updating asset with ID {Id}: {Message}", id, ex.Message);
            return Result.Fail<Asset>($"Error updating asset: {ex.Message}");
        }
    }

    public async Task<Result<Asset>> UpdateAsync(long id, UpdateAssetDto updateDto, long userId)
    {
        updateDto.UpdatedById = userId;
        return await this.UpdateAsync(id, updateDto);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        Result<Asset> assetResult = await this._assetRepository.GetByIdAsync(id);
        if (assetResult.IsFailed)
        {
            this._logger.LogError("Failed to find asset with id {Id} for deletion. Errors: {Errors}", id,
                assetResult.Errors);
            return false;
        }

        Result deleteResult = await this._assetRepository.DeleteAsync(id);
        return deleteResult.IsSuccess;
    }

    public async Task<Result<AssetResponseDto>> FindAsync(QueryParamsDto findParams)
    {
        string searchTerm = findParams.SearchTerm ?? string.Empty;
        if (string.IsNullOrEmpty(searchTerm) || searchTerm.Length < 3)
        {
            return Result.Fail<AssetResponseDto>("Search term is required and must be at least 3 characters long");
        }

        Result<(IEnumerable<Asset> Assets, int TotalCount)> assetsResult =
            await this._assetRepository.FindByConditionAsync(searchTerm);

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
        Result<(IEnumerable<Asset> Items, int Total)> searchResult =
            await this._assetRepository.SearchAsync(searchParams);
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

    public Result<CreateAssetDto> ValidateCreateAssetDto(CreateAssetDto dto)
    {
        if (string.IsNullOrEmpty(dto.Name))
        {
            return Result.Fail<CreateAssetDto>("Name is required");
        }

        if (dto.Asset == null || dto.Asset.Length == 0)
        {
            return Result.Fail<CreateAssetDto>("Asset file is required and cannot be empty");
        }

        if (dto.WorkAssetId.HasValue && dto.WorkAssetId.Value <= 0)
        {
            return Result.Fail<CreateAssetDto>("Work asset ID must be greater than 0");
        }

        if (dto.Asset.Length > Constants.MAX_FILE_SIZE_BYTES)
        {
            return Result.Fail<CreateAssetDto>(
                $"File size exceeds maximum limit of {Constants.MAX_FILE_SIZE_MB}MB");
        }

        return Result.Ok(dto);
    }

    public Result<UpdateAssetDto> ValidateUpdateAssetDto(UpdateAssetDto dto)
    {
        if (string.IsNullOrEmpty(dto.Name))
        {
            return Result.Fail<UpdateAssetDto>("Name is required");
        }

        if (dto.File is null)
        {
            return Result.Fail<UpdateAssetDto>("File is required");
        }

        return Result.Ok(dto);
    }
}
