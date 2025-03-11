namespace zora.Core.DTOs.Requests;

public sealed class DynamicQueryPermissionParamsDto : DynamicQueryParamsDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? Description { get; set; }
    public string? PermissionString { get; set; }
    public string? RoleIds { get; set; }
    public string? WorkItemIds { get; set; }

    protected override Dictionary<string, string> GetParameters()
    {
        return new Dictionary<string, string>
        {
            { "Id", this.Id },
            { "Name", this.Name },
            { "CreatedAt", this.CreatedAt.ToString() },
            { "Description", this.Description },
            { "PermissionString", this.PermissionString },
            { "RoleIds", this.RoleIds },
            { "WorkItemIds", this.WorkItemIds }
        };
    }
}
