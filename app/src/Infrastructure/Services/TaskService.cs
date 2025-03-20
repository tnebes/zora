#region

using AutoMapper;
using FluentResults;
using zora.Core.Attributes;
using zora.Core.Domain;
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

    public Task<Result<ZoraTask>> GetByIdAsync(long id, long userId, bool includeProperties = false)
    {
        try
        {
            ZoraTask dummyTask = new ZoraTask
            {
                Id = id,
                Name = $"Task {id}",
                Description = "Dummy task description",
                Status = "In Progress",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-5),
                CreatedById = userId,
                UpdatedById = userId,
                ProjectId = 1,
                Priority = "Medium",
                ParentTaskId = null
            };

            return Task.FromResult(Result.Ok(dummyTask));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting task by id {Id}", id);
            return Task.FromResult(Result.Fail<ZoraTask>($"Error getting task by id {id}"));
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

    public Task<Result<ZoraTask>> UpdateAsync(long id, UpdateTaskDto updateDto, long userId)
    {
        try
        {
            ZoraTask updatedTask = new ZoraTask
            {
                Id = id,
                Name = "Updated Task",
                Description = "Dummy updated task description",
                Status = "Updated",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow,
                CreatedById = 1,
                UpdatedById = userId,
                ProjectId = 1,
                Priority = "High"
            };

            return Task.FromResult(Result.Ok(updatedTask));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error updating task with id {Id}", id);
            return Task.FromResult(Result.Fail<ZoraTask>($"Error updating task with id {id}"));
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
