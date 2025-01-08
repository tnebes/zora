#region

using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Requests.Interfaces;
using zora.Core.Enums;

#endregion

namespace zora.Core.Interfaces.Services;

public interface IQueryService
{
    void NormaliseQueryParams(IQueryParamsDto queryParams);
    void ValidateQueryParams(DynamicQueryParamsDto queryParams, ResourceType type);
}
