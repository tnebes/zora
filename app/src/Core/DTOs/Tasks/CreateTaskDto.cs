#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace zora.Core.DTOs.Tasks;

public sealed class CreateTaskDto
{
    [Required] [StringLength(100)] public string Name { get; set; } = string.Empty;

    [StringLength(1000)] public string? Description { get; set; }

    [Required] [StringLength(50)] public string Status { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    [Range(0, 100)] public decimal? CompletionPercentage { get; set; }

    [Range(0, double.MaxValue)] public decimal? EstimatedHours { get; set; }

    [Range(0, double.MaxValue)] public decimal? ActualHours { get; set; }

    public long? AssigneeId { get; set; }

    public long? ProjectId { get; set; }

    [StringLength(20)] public string? Priority { get; set; }

    public long? ParentTaskId { get; set; }
}
