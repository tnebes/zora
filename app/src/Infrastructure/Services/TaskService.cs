#region

using AutoMapper;
using FluentResults;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs.Permissions;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Tasks;
using zora.Core.Enums;
using zora.Core.Interfaces.Repositories;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public sealed class TaskService : ITaskService, IZoraService
{
    private readonly IAuthorisationService _authorisationService;
    private readonly ILogger<TaskService> _logger;
    private readonly IMapper _mapper;
    private readonly IRoleRepository _roleRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;

    public TaskService(IAuthorisationService authorisationService, ITaskRepository taskRepository, IMapper mapper,
        ILogger<TaskService> logger, IUserRepository userRepository, IRoleRepository roleRepository) =>
        (this._authorisationService, this._taskRepository, this._mapper, this._logger, this._userRepository,
            this._roleRepository) =
        (authorisationService, taskRepository, mapper, logger, userRepository, roleRepository);

    public async Task<Result<(IEnumerable<ZoraTask>, int total)>> GetAsync(QueryParamsDto queryParams, long userId)
    {
        try
        {
            IQueryable<ZoraTask> query = this._taskRepository.GetQueryable();

            IQueryable<ZoraTask> filteredQuery =
                await this._authorisationService.FilterByPermission(query, userId, PermissionFlag.Read);

            Result<(IEnumerable<ZoraTask>, int total)> tasksResult =
                await this._taskRepository.GetPagedAsync(filteredQuery, queryParams.Page, queryParams.PageSize);

            if (tasksResult.IsFailed)
            {
                this._logger.LogError("Task Service failed to receive tasks. Errors: {Errors}", tasksResult.Errors);
                return Result.Fail<(IEnumerable<ZoraTask>, int total)>(tasksResult.Errors);
            }

            return tasksResult;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting tasks");
            return Result.Fail<(IEnumerable<ZoraTask>, int total)>("Error getting tasks");
        }
    }

    public async Task<Result<TaskResponseDto>> GetDtoAsync(QueryParamsDto queryParams, long userId)
    {
        Result<(IEnumerable<ZoraTask>, int total)> tasksResult = await this.GetAsync(queryParams, userId);

        if (tasksResult.IsFailed)
        {
            this._logger.LogError("Task Service failed to receive tasks. Errors: {Errors}", tasksResult.Errors);
            return Result.Fail<TaskResponseDto>(tasksResult.Errors);
        }

        (IEnumerable<ZoraTask> tasks, int total) = tasksResult.Value;
        IEnumerable<ReadTaskDto> taskDtos = this._mapper.Map<IEnumerable<ReadTaskDto>>(tasks);
        TaskResponseDto taskResponseDto = new TaskResponseDto
        {
            Total = total,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            Items = taskDtos
        };
        return Result.Ok(taskResponseDto);
    }

    public async Task<Result<ZoraTask>> GetByIdAsync(long id, long userId, bool includeProperties = false)
    {
        try
        {
            PermissionRequestDto permissionRequest = new PermissionRequestDto
            {
                ResourceId = id,
                ResourceType = ResourceType.Task,
                RequestedPermission = PermissionFlag.Read,
                UserId = userId
            };

            if (!await this._authorisationService.IsAuthorisedAsync(permissionRequest))
            {
                this._logger.LogInformation("User {UserId} is not authorised to access task {TaskId}", userId, id);
                return Result.Fail<ZoraTask>("User is not authorised to access task");
            }

            Result<ZoraTask> result = await this._taskRepository.GetByIdAsync(id, includeProperties);

            if (result.IsFailed)
            {
                this._logger.LogError("Task Service failed to receive task. Errors: {Errors}", result.Errors);
                return Result.Fail<ZoraTask>(result.Errors);
            }

            return Result.Ok(result.Value);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting task by id {Id}", id);
            return Result.Fail<ZoraTask>($"Error getting task by id {id}");
        }
    }

    public Task<Result<ZoraTask>> CreateAsync(CreateTaskDto createDto, long userId)
    {
        try
        {
            ZoraTask newTask = new ZoraTask
            {
                Id = new Random().Next(1, 1000),
                Name = "New Task",
                Description = "Dummy created task",
                Status = "New",
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId,
                ProjectId = 1,
                Priority = "Medium"
            };

            return Task.FromResult(Result.Ok(newTask));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error creating task");
            return Task.FromResult(Result.Fail<ZoraTask>("Error creating task"));
        }
    }

    public async Task<Result<ZoraTask>> UpdateAsync(long id, UpdateTaskDto updateDto, long userId)
    {
        try
        {
            PermissionRequestDto permissionRequest = new PermissionRequestDto
            {
                ResourceId = id,
                ResourceType = ResourceType.Task,
                RequestedPermission = PermissionFlag.Write,
                UserId = userId
            };

            if (!await this._authorisationService.IsAuthorisedAsync(permissionRequest))
            {
                this._logger.LogInformation("User {UserId} is not authorised to update task {TaskId}", userId, id);
                return Result.Fail<ZoraTask>("User is not authorised to update task");
            }

            Result<ZoraTask> taskResult = await this._taskRepository.GetByIdAsync(id, true);
            
            if (taskResult.IsFailed)
            {
                this._logger.LogError("Failed to retrieve task for update. Errors: {Errors}", taskResult.Errors);
                return Result.Fail<ZoraTask>(taskResult.Errors);
            }

            ZoraTask existingTask = taskResult.Value;
            
            this._mapper.Map(updateDto, existingTask);
            existingTask.UpdatedAt = DateTime.UtcNow;
            existingTask.UpdatedById = userId;
            
            Result<ZoraTask> updateResult = await this._taskRepository.UpdateAsync(existingTask);
            
            if (updateResult.IsFailed)
            {
                this._logger.LogError("Failed to update task. Errors: {Errors}", updateResult.Errors);
                return Result.Fail<ZoraTask>(updateResult.Errors);
            }

            return Result.Ok(updateResult.Value);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error updating task with id {Id}", id);
            return Result.Fail<ZoraTask>($"Error updating task with id {id}");
        }
    }

    public Task<bool> DeleteAsync(long id, long userId)
    {
        try
        {
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error deleting task with id {Id}", id);
            return Task.FromResult(false);
        }
    }

    public Task<Result<TaskResponseDto>> FindAsync(QueryParamsDto findParams, long userId)
    {
        try
        {
            ReadTaskDto dummyTaskDto = new ReadTaskDto
            {
                Id = 1,
                Name = "Found Task",
                Description = "Dummy found task",
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                CreatedById = userId,
                ProjectId = 1,
                Priority = "Medium"
            };

            TaskResponseDto response = new TaskResponseDto
            {
                Total = 1,
                Page = findParams.Page,
                PageSize = findParams.PageSize,
                Items = new List<ReadTaskDto> { dummyTaskDto }
            };

            return Task.FromResult(Result.Ok(response));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error finding tasks");
            return Task.FromResult(Result.Fail<TaskResponseDto>("Error finding tasks"));
        }
    }

    public Task<Result<TaskResponseDto>> SearchAsync(DynamicQueryTaskParamsDto searchParams, long userId)
    {
        try
        {
            List<ReadTaskDto> dummyTasks = new List<ReadTaskDto>
            {
                new()
                {
                    Id = 1,
                    Name = "Search Result Task 1",
                    Description = "Dummy search result 1",
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    CreatedById = userId,
                    ProjectId = 1,
                    Priority = "Medium"
                },
                new()
                {
                    Id = 2,
                    Name = "Search Result Task 2",
                    Description = "Dummy search result 2",
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    CreatedById = userId,
                    ProjectId = 1,
                    Priority = "High"
                }
            };

            TaskResponseDto response = new TaskResponseDto
            {
                Total = dummyTasks.Count,
                Page = searchParams.Page,
                PageSize = searchParams.PageSize,
                Items = dummyTasks
            };

            return Task.FromResult(Result.Ok(response));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error searching tasks");
            return Task.FromResult(Result.Fail<TaskResponseDto>("Error searching tasks"));
        }
    }
}
