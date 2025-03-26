#region

using zora.Core.DTOs.Requests.Interfaces;

#endregion

namespace zora.Core.DTOs.Requests;

public sealed class QueryParamsDto : IQueryParamsDto
{
    public string? SearchTerm { get; set; }
    public string? SortColumn { get; set; }
    public string? SortDirection { get; set; }
    public long? WorkAssetId { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }

    public string ToQueryString()
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>
        {
            { "searchTerm", this.SearchTerm },
            { "sortColumn", this.SortColumn },
            { "sortDirection", this.SortDirection },
            { "page", this.Page.ToString() },
            { "pageSize", this.PageSize.ToString() },
            { "workAssetId", this.WorkAssetId?.ToString() }
        };

        List<string> queryParams = parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}")
            .ToList();

        return queryParams.Any() ? $"?{string.Join("&", queryParams)}" : string.Empty;
    }
}
