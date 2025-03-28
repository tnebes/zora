#region

using FluentResults;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Tasks;

#endregion

namespace zora.Core.Interfaces.Services;

public interface ITaskService : IBaseServiceWithPermissionFilter<ZoraTask, CreateTaskDto, UpdateTaskDto, TaskResponseDto
    ,
    DynamicQueryTaskParamsDto>
{
    Task<Result<ZoraTask>> UpdateEntityAsync(ZoraTask task, long userId);
    Task<Result<TaskResponseDto>> GetPriorityTasksAsync(long userId);
}
