#region

using zora.Core.DTOs.Requests;

#endregion

namespace zora.Core.Interfaces.Services;

public interface IQueryService
{
    void NormaliseQueryParams(QueryParamsDto queryParams);
}
