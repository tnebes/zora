namespace zora.Core.DTOs.Requests;

public sealed class DynamicQueryRoleParamsDto : DynamicQueryParamsDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? Permission { get; set; }
    public string? User { get; set; }

    protected override Dictionary<string, string> GetParameters()
    {
        return new Dictionary<string, string>
        {
            { "id", this.Id },
            { "name", this.Name },
            { "createdAt", this.CreatedAt?.ToString() },
            { "permission", this.Permission },
            { "user", this.User }
        };
    }
}
