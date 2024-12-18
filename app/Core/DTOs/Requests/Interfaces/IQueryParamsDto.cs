using zora.Core.Enums;

namespace zora.Core.DTOs.Requests.Interfaces;

public interface IQueryParamsDto<T> where T : Enum
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortColumn { get; set; }
    public string? SortDirection { get; set; }
}
