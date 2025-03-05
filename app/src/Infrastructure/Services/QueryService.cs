#region

using zora.Core;
using zora.Core.Attributes;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Requests.Interfaces;
using zora.Core.Enums;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public sealed class QueryService : IQueryService, IZoraService
{
    private readonly ILogger<QueryService> _logger;
    public QueryService(ILogger<QueryService> logger) => this._logger = logger;

    public void NormaliseQueryParamsForAdmin(IQueryParamsDto queryParams)
    {
        queryParams.Page = Math.Max(Constants.DEFAULT_PAGE_SIZE, queryParams.Page);
        queryParams.PageSize = Math.Max(Constants.DEFAULT_PAGE_SIZE, queryParams.PageSize);
    }

    public void NormaliseQueryParams(IQueryParamsDto queryParams)
    {
        queryParams.Page = Math.Max(1, queryParams.Page);
        queryParams.PageSize = Math.Min(queryParams.PageSize <= 0 ? Constants.DEFAULT_PAGE_SIZE : queryParams.PageSize,
            Constants.DEFAULT_PAGE_SIZE);
    }

    public void ValidateQueryParams(DynamicQueryParamsDto queryParams, ResourceType type)
    {
        if (type == ResourceType.Route)
        {
            throw new ArgumentOutOfRangeException(nameof(type), "Invalid resource type");
        }

        if (queryParams.Page <= 0 || queryParams.PageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(queryParams), "Page and PageSize must be greater than 0");
        }
    }
}
