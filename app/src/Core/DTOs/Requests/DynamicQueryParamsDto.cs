#region

using zora.Core.DTOs.Requests.Interfaces;

#endregion

namespace zora.Core.DTOs.Requests;

public abstract class DynamicQueryParamsDto : IQueryParamsDto
{
    public required int Page { get; set; }
    public required int PageSize { get; set; }

    public string ToQueryString()
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>
        {
            { "page", this.Page.ToString() },
            { "pageSize", this.PageSize.ToString() }
        };

        foreach (var parameter in this.GetParameters())
        {
            parameters.TryAdd(parameter.Key, parameter.Value);
        }

        List<string> queryParams = parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}")
            .ToList();

        return queryParams.Any() ? $"?{string.Join("&", queryParams)}" : string.Empty;
    }

    protected abstract Dictionary<string, string> GetParameters();
}
