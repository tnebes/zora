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
public class TaskService : ITaskService, IZoraService
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
        IQueryable<ZoraTask> query = this._taskRepository.GetQueryable();

        // Apply authorization filter
        IQueryable<WorkItem> filteredWorkItems = await this._authorisationService.FilterByPermission(query, userId, PermissionFlag.Read);
        IQueryable<ZoraTask> filteredQuery = filteredWorkItems.OfType<ZoraTask>();

        Result<(IEnumerable<ZoraTask>, int total)> tasksResult =
            await this._taskRepository.GetPagedAsync(filteredQuery, queryParams.Page, queryParams.PageSize);

        if (tasksResult.IsFailed)
        {
            this._logger.LogError("Task Service failed to receive tasks. Errors: {Errors}", tasksResult.Errors);
            return Result.Fail<(IEnumerable<ZoraTask>, int total)>(tasksResult.Errors);
        }

        IEnumerable<ZoraTask> tasks = tasksResult.Value.Item1;
        int total = tasksResult.Value.Item2;

        // after getting the tasks, we need to check if the user has at least READ permissions to the tasks
        // hint: use AuthorisationService to check if the user has at least READ permissions to the tasks
        // only those tasks should be returned and all the logic should be in the AuthorisationService
        // logic:
        // 1. get query for all tasks
        // 2. filter tasks based on permissions connected with roles connected to the user
        // 3. return the filtered tasks and total count of tasks
        // requirement: do not query the database so as to encounter the n+1 problem or any other performance issues
        // hint: the AuthorisationService should be, if possible, deal with WorkItem types other than Task
        // because we will also have to deal with Program and Project types

        // additionally, this must account for cases when the user has a permission on a project or program so that all the tasks related to that project or program are also returned

        // BUG: the tasks are not being filtered based on permissions, only the existence of permissions is checked
        return tasksResult;
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

    public Task<Result<ZoraTask>> GetByIdAsync(long id, long userId, bool includeProperties = false) =>
        throw new NotImplementedException();

    public Task<Result<ZoraTask>> CreateAsync(CreateTaskDto createDto, long userId) =>
        throw new NotImplementedException();

    public Task<Result<ZoraTask>> UpdateAsync(long id, UpdateTaskDto updateDto, long userId) =>
        throw new NotImplementedException();

    public Task<bool> DeleteAsync(long id, long userId) => throw new NotImplementedException();

    public Task<Result<TaskResponseDto>> FindAsync(QueryParamsDto findParams, long userId) =>
        throw new NotImplementedException();

    public Task<Result<TaskResponseDto>> SearchAsync(DynamicQueryTaskParamsDto searchParams, long userId) =>
        throw new NotImplementedException();
}
