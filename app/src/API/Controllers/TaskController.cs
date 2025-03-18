#region

using System.ComponentModel;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zora.API.Extensions;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Tasks;
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

            this.QueryService.NormaliseQueryParams(queryParams);

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

    public override Task<ActionResult<ZoraTask>> Create(CreateTaskDto createDto) => throw new NotImplementedException();

    public override Task<ActionResult<ZoraTask>> Update(long id, UpdateTaskDto updateDto) =>
        throw new NotImplementedException();

    public override Task<ActionResult<bool>> Delete(long id) => throw new NotImplementedException();

    public override Task<ActionResult<TaskResponseDto>> Find(QueryParamsDto findParams) =>
        throw new NotImplementedException();

    public override Task<ActionResult<TaskResponseDto>> Search(DynamicQueryTaskParamsDto searchParams) =>
        throw new NotImplementedException();
}
