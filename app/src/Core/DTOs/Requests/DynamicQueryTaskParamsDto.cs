namespace zora.Core.DTOs.Requests;

public sealed class DynamicQueryTaskParamsDto : DynamicQueryParamsDto
{
    public string? Name { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public long? ProjectId { get; set; }

    protected override Dictionary<string, string> GetParameters()
    {
        try
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(this.Name))
            {
                parameters.Add("name", this.Name);
            }

            if (!string.IsNullOrEmpty(this.Status))
            {
                parameters.Add("status", this.Status);
            }

            if (!string.IsNullOrEmpty(this.Priority))
            {
                parameters.Add("priority", this.Priority);
            }

            if (this.ProjectId.HasValue)
            {
                parameters.Add("projectId", this.ProjectId.Value.ToString());
            }

            return parameters;
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }
}
