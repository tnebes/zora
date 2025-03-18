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
        this.CreateMap<WorkItem, WorkItemDto>();

        this.CreateMap<ZoraTask, ReadTaskDto>()
            .IncludeBase<WorkItem, WorkItemDto>()
            .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
            .ForMember(dest => dest.ParentTaskId, opt => opt.MapFrom(src => src.ParentTaskId));
    }
}
