#region

using FluentResults;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;

#endregion

namespace zora.Core.Interfaces.Services;

public interface IAssetService : IBaseService<Asset, CreateAssetDto, UpdateAssetDto, AssetResponseDto,
    DynamicQueryAssetParamsDto>
{
    Result<CreateAssetDto> ValidateCreateAssetDto(CreateAssetDto dto);

    Result<UpdateAssetDto> ValidateUpdateAssetDto(UpdateAssetDto dto);

}
