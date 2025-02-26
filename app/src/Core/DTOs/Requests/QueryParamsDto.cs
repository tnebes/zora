#region

using zora.Core.DTOs.Requests.Interfaces;

#endregion

namespace zora.Core.DTOs.Requests;

public sealed class QueryParamsDto : IQueryParamsDto
{
    public string? SearchTerm { get; set; }
    public string? SortColumn { get; set; }
    public string? SortDirection { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
