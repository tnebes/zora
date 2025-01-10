#region

using FluentResults;
using zora.Core.Domain;
using zora.Core.DTOs;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Requests.Interfaces;
using zora.Core.DTOs.Responses;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.Infrastructure.Services;

public sealed class AssetService : IAssetService, IZoraService
{
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
}
