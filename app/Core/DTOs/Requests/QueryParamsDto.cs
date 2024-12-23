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

    /*
     * public class QueryParamsDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 10;

    [MaxLength(100)]
    public string? SearchTerm { get; set; }

    [RegularExpression("^(username|email|createdat)$", ErrorMessage = "Invalid sort column")]
    public string? SortColumn { get; set; }

    [RegularExpression("^(asc|desc)$", ErrorMessage = "Sort direction must be 'asc' or 'desc'")]
    public string? SortDirection { get; set; } = "asc";
}
     */
}
