#region

using AutoMapper;
using zora.Core.Domain;
using zora.Core.DTOs.Tasks;
using zora.Core.DTOs.WorkItems;

#endregion

namespace zora.API.Mapping;

public sealed class TaskMappingProfile : Profile
{
    public TaskMappingProfile()
    {
        this.CreateMap<WorkItem, WorkItemDto>()
            .ForMember(dest => dest.AssigneeName, opt => opt.MapFrom(src => src.Assignee != null ? src.Assignee.Username : null));

        this.CreateMap<ZoraTask, ReadTaskDto>()
            .IncludeBase<WorkItem, WorkItemDto>()
            .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
            .ForMember(dest => dest.ParentTaskId, opt => opt.MapFrom(src => src.ParentTaskId));

        this.CreateMap<UpdateTaskDto, ZoraTask>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate))
            .ForMember(dest => dest.CompletionPercentage, opt => opt.MapFrom(src => src.CompletionPercentage))
            .ForMember(dest => dest.EstimatedHours, opt => opt.MapFrom(src => src.EstimatedHours))
            .ForMember(dest => dest.ActualHours, opt => opt.MapFrom(src => src.ActualHours))
            .ForMember(dest => dest.AssigneeId, opt => opt.MapFrom(src => src.AssigneeId))
            .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
            .ForMember(dest => dest.ParentTaskId, opt => opt.MapFrom(src => src.ParentTaskId))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedById, opt => opt.Ignore())
            .ForMember(dest => dest.Assignee, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Project, opt => opt.Ignore())
            .ForMember(dest => dest.ParentTask, opt => opt.Ignore())
            .ForMember(dest => dest.SubTasks, opt => opt.Ignore())
            .ForMember(dest => dest.SourceRelationships, opt => opt.Ignore())
            .ForMember(dest => dest.TargetRelationships, opt => opt.Ignore())
            .ForMember(dest => dest.WorkItemAssets, opt => opt.Ignore())
            .ForMember(dest => dest.PermissionWorkItems, opt => opt.Ignore());

        this.CreateMap<ZoraTask, UpdateTaskDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate))
            .ForMember(dest => dest.CompletionPercentage, opt => opt.MapFrom(src => src.CompletionPercentage))
            .ForMember(dest => dest.EstimatedHours, opt => opt.MapFrom(src => src.EstimatedHours))
            .ForMember(dest => dest.ActualHours, opt => opt.MapFrom(src => src.ActualHours))
            .ForMember(dest => dest.AssigneeId, opt => opt.MapFrom(src => src.AssigneeId))
            .ForMember(dest => dest.AssigneeName, opt => opt.MapFrom(src => src.Assignee != null ? src.Assignee.Username : null))
            .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
            .ForMember(dest => dest.ParentTaskId, opt => opt.MapFrom(src => src.ParentTaskId));
    }
}
