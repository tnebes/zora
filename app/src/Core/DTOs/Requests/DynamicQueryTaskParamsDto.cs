using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace zora.Core.DTOs.Requests;

public sealed class DynamicQueryTaskParamsDto : DynamicQueryParamsDto
{
    [FromQuery(Name = "searchTerm")]
    [JsonPropertyName("searchTerm")]
    public string? SearchTerm { get; set; }

    [FromQuery(Name = "name")]
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [FromQuery(Name = "status")]
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [FromQuery(Name = "priority")]
    [JsonPropertyName("priority")]
    public string? Priority { get; set; }

    [FromQuery(Name = "projectId")]
    [JsonPropertyName("projectId")]
    public long? ProjectId { get; set; }

    [FromQuery(Name = "assigneeId")]
    [JsonPropertyName("assigneeId")]
    public long? AssigneeId { get; set; }

    [FromQuery(Name = "sortColumn")]
    [JsonPropertyName("sortColumn")]
    public string? SortColumn { get; set; }

    [FromQuery(Name = "sortDirection")]
    [JsonPropertyName("sortDirection")]
    public string? SortDirection { get; set; }

    protected override Dictionary<string, string> GetParameters()
    {
        try
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(this.SearchTerm))
            {
                parameters.Add("searchTerm", this.SearchTerm);
            }

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

            if (this.AssigneeId.HasValue)
            {
                parameters.Add("assigneeId", this.AssigneeId.Value.ToString());
            }

            if (!string.IsNullOrEmpty(this.SortColumn))
            {
                parameters.Add("sortColumn", this.SortColumn);
            }

            if (!string.IsNullOrEmpty(this.SortDirection))
            {
                parameters.Add("sortDirection", this.SortDirection);
            }

            return parameters;
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }
}
