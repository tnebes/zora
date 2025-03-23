#region

using System.ComponentModel;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zora.API.Extensions;
using zora.Core.Domain;
using zora.Core.DTOs.Permissions;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Tasks;
using zora.Core.Enums;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/tasks")]
[Produces("application/json")]
[Description("Tasks API")]
public sealed class TaskController : BaseCrudController<ZoraTask, CreateTaskDto, UpdateTaskDto,
    TaskResponseDto, DynamicQueryTaskParamsDto>, IZoraService
{
    private readonly IAssetService _assetService;
    private readonly IAuthorisationService _authorisationService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<TaskController> _logger;
    private readonly IMapper _mapper;
    private readonly ITaskService _taskService;

    public TaskController(
        IAssetService assetService,
        IQueryService queryService,
        IRoleService roleService,
        IJwtService jwtService,
        IAuthorisationService authorisationService,
        ILogger<TaskController> logger,
        IMapper mapper,
        ITaskService taskService)
        : base(logger, roleService, queryService)
    {
        this._assetService = assetService;
        this._jwtService = jwtService;
        this._authorisationService = authorisationService;
        this._mapper = mapper;
        this._logger = logger;
        this._taskService = taskService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<TaskResponseDto>> Get([FromQuery] QueryParamsDto queryParams)
    {
        try
        {
            if (!this.User.Identity?.IsAuthenticated ?? true)
            {
                this.LogUnauthorisedAccess(this.HttpContext.User);
                return this.Unauthorized();
            }

            long userId = this.HttpContext.User.GetUserId();

            Result<TaskResponseDto> result = await this._taskService.GetDtoAsync(queryParams, userId);

            if (result.IsFailed)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, result.Errors);
            }

            return this.Ok(result.Value);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting tasks");
            return this.BadRequest(ex.Message);
        }
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReadTaskDto>> GetSingle(long id)
    {
        try
        {
            if (!this.User.Identity?.IsAuthenticated ?? true)
            {
                this.LogUnauthorisedAccess(this.HttpContext.User);
                return this.Unauthorized();
            }

            long userId = this.HttpContext.User.GetUserId();

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
                return this.Forbid();
            }

            Result<ZoraTask> result = await this._taskService.GetByIdAsync(id, userId);

            if (result.IsFailed)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, result.Errors);
            }

            ZoraTask task = result.Value;

            ReadTaskDto taskDto = this._mapper.Map<ReadTaskDto>(task);

            return this.Ok(taskDto);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting task with id {Id}", id);
            return this.BadRequest(ex.Message);
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<ZoraTask>> Create(CreateTaskDto createDto)
    {
        try
        {
            if (!this.User.Identity?.IsAuthenticated ?? true)
            {
                this.LogUnauthorisedAccess(this.HttpContext.User);
                return this.Unauthorized();
            }

            long userId = this.HttpContext.User.GetUserId();
            Result<ZoraTask> result = await this._taskService.CreateAsync(createDto, userId);

            if (result.IsFailed)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, result.Errors);
            }

            return this.CreatedAtAction(nameof(this.Get), new { id = result.Value.Id }, result.Value);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error creating task");
            return this.BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<ZoraTask>> Update(long id, UpdateTaskDto updateDto)
    {
        try
        {
            if (!this.User.Identity?.IsAuthenticated ?? true)
            {
                this.LogUnauthorisedAccess(this.HttpContext.User);
                return this.Unauthorized();
            }

            long userId = this.HttpContext.User.GetUserId();
            Result<ZoraTask> result = await this._taskService.UpdateAsync(id, updateDto, userId);

            if (result.IsFailed)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, result.Errors);
            }

            return this.Ok(result.Value);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error updating task with id {Id}", id);
            return this.BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<bool>> Delete(long id)
    {
        try
        {
            if (!this.User.Identity?.IsAuthenticated ?? true)
            {
                this.LogUnauthorisedAccess(this.HttpContext.User);
                return this.Unauthorized();
            }

            long userId = this.HttpContext.User.GetUserId();
            bool result = await this._taskService.DeleteAsync(id, userId);

            if (!result)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete task");
            }

            return this.Ok(result);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error deleting task with id {Id}", id);
            return this.BadRequest(ex.Message);
        }
    }

    [HttpGet("find")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<TaskResponseDto>> Find(QueryParamsDto findParams)
    {
        try
        {
            if (!this.User.Identity?.IsAuthenticated ?? true)
            {
                this.LogUnauthorisedAccess(this.HttpContext.User);
                return this.Unauthorized();
            }

            this.QueryService.NormaliseQueryParams(findParams);

            long userId = this.HttpContext.User.GetUserId();
            Result<TaskResponseDto> result = await this._taskService.FindAsync(findParams, userId);

            if (result.IsFailed)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, result.Errors);
            }

            return this.Ok(result.Value);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error finding tasks");
            return this.BadRequest(ex.Message);
        }
    }

    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<TaskResponseDto>> Search(DynamicQueryTaskParamsDto searchParams)
    {
        try
        {
            if (!this.User.Identity?.IsAuthenticated ?? true)
            {
                this.LogUnauthorisedAccess(this.HttpContext.User);
                return this.Unauthorized();
            }

            this.QueryService.NormaliseQueryParams(searchParams);

            long userId = this.HttpContext.User.GetUserId();
            Result<TaskResponseDto> result = await this._taskService.SearchAsync(searchParams, userId);

            if (result.IsFailed)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, result.Errors);
            }

            return this.Ok(result.Value);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error searching tasks");
            return this.BadRequest(ex.Message);
        }
    }
}
