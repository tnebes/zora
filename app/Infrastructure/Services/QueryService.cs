#region

using zora.Core;
using zora.Core.Attributes;
using zora.Core.DTOs.Requests;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public class QueryService : IQueryService, IZoraService
{
    public void NormaliseQueryParams(QueryParamsDto queryParams)
    {
        queryParams.Page = Math.Max(1, queryParams.Page);
        queryParams.PageSize = Math.Clamp(queryParams.PageSize, Constants.DEFAULT_PAGE_SIZE,
            Constants.MAX_RESULTS_PER_PAGE);
    }
}
