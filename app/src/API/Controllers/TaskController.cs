#region

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Tasks;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.API.Controllers;

public sealed class TaskController : BaseCrudController<ZoraTask, CreateTaskDto, UpdateTaskDto,
    TaskResponseDto, DynamicQueryTaskParamsDto>, IZoraService
{
    private readonly IAssetService _assetService;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;

    public TaskController(
        IAssetService assetService,
        IQueryService queryService,
        IRoleService roleService,
        IJwtService jwtService,
        ILogger<TaskController> logger,
        IMapper mapper)
        : base(logger, roleService, queryService)
    {
        this._assetService = assetService;
        this._jwtService = jwtService;
        this._mapper = mapper;
    }

    public override Task<ActionResult<TaskResponseDto>> Get(QueryParamsDto queryParams) =>
        throw new NotImplementedException();

    public override Task<ActionResult<ZoraTask>> Create(CreateTaskDto createDto) => throw new NotImplementedException();

    public override Task<ActionResult<ZoraTask>> Update(long id, UpdateTaskDto updateDto) =>
        throw new NotImplementedException();

    public override Task<ActionResult<bool>> Delete(long id) => throw new NotImplementedException();

    public override Task<ActionResult<TaskResponseDto>> Find(QueryParamsDto findParams) =>
        throw new NotImplementedException();

    public override Task<ActionResult<TaskResponseDto>> Search(DynamicQueryTaskParamsDto searchParams) =>
        throw new NotImplementedException();
}
