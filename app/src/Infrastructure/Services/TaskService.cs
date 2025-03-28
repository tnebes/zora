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
using Microsoft.EntityFrameworkCore;

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

    public async Task<Result<ZoraTask>> CreateAsync(CreateTaskDto createDto, long userId)
    {
        try
        {
            ZoraTask newTask = this._mapper.Map<ZoraTask>(createDto);
            newTask.CreatedAt = DateTime.UtcNow;
            newTask.CreatedById = userId;
            newTask.UpdatedAt = DateTime.UtcNow;
            newTask.UpdatedById = userId;

            Result<ZoraTask> createResult = await this._taskRepository.CreateAsync(newTask);

            if (createResult.IsFailed)
            {
                this._logger.LogError("Failed to create task. Errors: {Errors}", createResult.Errors);
                return Result.Fail<ZoraTask>(createResult.Errors);
            }

            return Result.Ok(createResult.Value);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error creating task");
            return Result.Fail<ZoraTask>("Error creating task");
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

    public async Task<bool> DeleteAsync(long id, long userId)
    {
        try
        {
            PermissionRequestDto permissionRequest = new PermissionRequestDto
            {
                ResourceId = id,
                ResourceType = ResourceType.Task,
                RequestedPermission = PermissionFlag.Delete,
                UserId = userId
            };

            if (!await this._authorisationService.IsAuthorisedAsync(permissionRequest))
            {
                this._logger.LogInformation("User {UserId} is not authorised to delete task {TaskId}", userId, id);
                return false;
            }

            Result<ZoraTask> taskResult = await this._taskRepository.GetByIdAsync(id, true);

            if (taskResult.IsFailed)
            {
                this._logger.LogError("Failed to retrieve task for deletion. Errors: {Errors}", taskResult.Errors);
                return false;
            }

            ZoraTask taskToDelete = taskResult.Value;
            Result<bool> deleteResult = await this._taskRepository.DeleteAsync(taskToDelete);

            if (deleteResult.IsFailed)
            {
                this._logger.LogError("Failed to delete task. Errors: {Errors}", deleteResult.Errors);
                return false;
            }

            return deleteResult.Value;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error deleting task with id {Id}", id);
            return false;
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

    public async Task<Result<TaskResponseDto>> SearchAsync(DynamicQueryTaskParamsDto searchParams, long userId)
    {
        try
        {
            Result<(IEnumerable<ZoraTask>, int total)> searchResult;
            
            if (searchParams.Ids != null && searchParams.Ids.Length > 0)
            {
                IQueryable<ZoraTask> query = this._taskRepository.GetQueryable();
                query = query.Where(t => searchParams.Ids.Contains(t.Id));
                
                IQueryable<ZoraTask> filteredQuery = 
                    await this._authorisationService.FilterByPermission(query, userId, PermissionFlag.Read);
                
                int totalCount = await filteredQuery.CountAsync();
                
                int skip = (searchParams.Page - 1) * searchParams.PageSize;
                IEnumerable<ZoraTask> filteredTasks = await filteredQuery
                    .Include(t => t.Assignee)
                    .Include(t => t.CreatedBy)
                    .Include(t => t.UpdatedBy)
                    .Skip(skip)
                    .Take(searchParams.PageSize)
                    .ToListAsync();
                
                searchResult = Result.Ok((filteredTasks, totalCount));
            }
            else
            {
                searchResult = await this._taskRepository.SearchAsync(searchParams, true);
            }

            if (searchResult.IsFailed)
            {
                this._logger.LogError("Failed to search tasks. Errors: {Errors}", searchResult.Errors);
                return Result.Fail<TaskResponseDto>(searchResult.Errors);
            }

            (IEnumerable<ZoraTask> tasks, int total) = searchResult.Value;
            IEnumerable<ReadTaskDto> taskDtos = this._mapper.Map<IEnumerable<ReadTaskDto>>(tasks);

            TaskResponseDto response = new TaskResponseDto
            {
                Total = total,
                Page = searchParams.Page,
                PageSize = searchParams.PageSize,
                Items = taskDtos
            };

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error searching tasks");
            return Result.Fail<TaskResponseDto>("Error searching tasks");
        }
    }

    public async Task<Result<ZoraTask>> UpdateEntityAsync(ZoraTask task, long userId)
    {
        try
        {
            task.UpdatedAt = DateTime.UtcNow;
            task.UpdatedById = userId;

            Result<ZoraTask> updateResult = await this._taskRepository.UpdateAsync(task);

            if (updateResult.IsFailed)
            {
                this._logger.LogError("Failed to update task. Errors: {Errors}", updateResult.Errors);
                return Result.Fail<ZoraTask>(updateResult.Errors);
            }

            return Result.Ok(updateResult.Value);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error updating task with id {Id}", task.Id);
            return Result.Fail<ZoraTask>($"Error updating task with id {task.Id}");
        }
    }
}
