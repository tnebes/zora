namespace zora.Core.DTOs.Requests;

public sealed class DynamicQueryAssetParamsDto : DynamicQueryParamsDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? AssetPath { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public string? WorkItemId { get; set; }
    public string? WorkItemName { get; set; }

    protected override Dictionary<string, string> GetParameters()
    {
        return new Dictionary<string, string>
        {
            { "id", this.Id },
            { "name", this.Name },
            { "description", this.Description },
            { "assetPath", this.AssetPath },
            { "createdBy", this.CreatedBy },
            { "updatedBy", this.UpdatedBy },
            { "workItemId", this.WorkItemId },
            { "workItemName", this.WorkItemName }
        };
    }
}
