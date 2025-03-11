#region

using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Requests.Interfaces;
using zora.Core.Enums;

#endregion

namespace zora.Core.Interfaces.Services;

public interface IQueryService
{
    void NormaliseQueryParamsForAdmin(IQueryParamsDto queryParams);
    void NormaliseQueryParams(IQueryParamsDto queryParams);
    void ValidateQueryParams(DynamicQueryParamsDto queryParams, ResourceType type);
}
