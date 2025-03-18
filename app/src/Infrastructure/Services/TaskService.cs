#region

using AutoMapper;
using FluentResults;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Tasks;
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

        IQueryable<ZoraTask> filteredQuery = query.Where(task =>
            task.PermissionWorkItems.Any(pwi =>
                pwi.Permission.RolePermissions.Any(rp =>
                    rp.Role.UserRoles.Any(ur =>
                        ur.UserId == userId
                    )
                )
            )
        );

        Result<(IEnumerable<ZoraTask>, int total)> tasksResult =
            await this._taskRepository.GetPagedAsync(filteredQuery, queryParams.Page, queryParams.PageSize);

        if (tasksResult.IsFailed)
        {
            this._logger.LogError("Task Service failed to receive tasks. Errors: {Errors}", tasksResult.Errors);
            return Result.Fail<(IEnumerable<ZoraTask>, int total)>(tasksResult.Errors);
        }

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
