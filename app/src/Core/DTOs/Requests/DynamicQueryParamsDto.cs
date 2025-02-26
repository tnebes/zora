#region

using zora.Core.DTOs.Requests.Interfaces;

#endregion

namespace zora.Core.DTOs.Requests;

public class DynamicQueryParamsDto : IQueryParamsDto
{
    public required int Page { get; set; }
    public required int PageSize { get; set; }
}
