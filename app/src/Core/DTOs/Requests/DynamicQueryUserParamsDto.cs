namespace zora.Core.DTOs.Requests;

public sealed class DynamicQueryUserParamsDto : DynamicQueryParamsDto
{
    public string? Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? Permission { get; set; }

    public override string ToQueryString()
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>
        {
            { "page", this.Page.ToString() },
            { "pageSize", this.PageSize.ToString() },
            { "id", this.Id },
            { "username", this.Username },
            { "email", this.Email },
            { "role", this.Role },
            { "createdAt", this.CreatedAt.ToString() },
            { "permission", this.Permission }
        };

        List<string> queryParams = parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}")
            .ToList();

        return queryParams.Any() ? $"?{string.Join("&", queryParams)}" : string.Empty;
    }
}
