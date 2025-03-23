namespace zora.Core.DTOs.Tasks;

public sealed class UpdateTaskDto
{
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public string Status { get; set; } = string.Empty;
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    public decimal? CompletionPercentage { get; set; }
    
    public decimal? EstimatedHours { get; set; }
    
    public decimal? ActualHours { get; set; }
    
    public long? AssigneeId { get; set; }
    
    public long? ProjectId { get; set; }
    
    public string? Priority { get; set; }
    
    public long? ParentTaskId { get; set; }
}
