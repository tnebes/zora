#region

using Microsoft.AspNetCore.Mvc;
using zora.API.Interfaces;
using zora.Core;
using zora.Core.DTOs.Requests;
using zora.Core.Interfaces.Services;

#endregion

public abstract class BaseCrudController<TEntity, TCreateDto, TUpdateDto, TResponseDto> : ControllerBase,
    ICrudController<TEntity, TCreateDto, TUpdateDto, TResponseDto>
{
    protected readonly ILogger<BaseCrudController<TEntity, TCreateDto, TUpdateDto, TResponseDto>> Logger;
    protected readonly IQueryService QueryService;
    protected readonly IRoleService RoleService;

    protected BaseCrudController(
        ILogger<BaseCrudController<TEntity, TCreateDto, TUpdateDto, TResponseDto>> logger,
        IRoleService roleService,
        IQueryService queryService)
    {
        this.Logger = logger;
        this.RoleService = roleService;
        this.QueryService = queryService;
    }

    public abstract Task<ActionResult<TResponseDto>> Get([FromQuery] QueryParamsDto queryParams);
    public abstract Task<ActionResult<TEntity>> Create([FromBody] TCreateDto createDto);
    public abstract Task<ActionResult<TEntity>> Update(long id, [FromBody] TUpdateDto updateDto);
    public abstract Task<ActionResult<bool>> Delete(long id);
    public abstract Task<ActionResult<TResponseDto>> Find([FromQuery] QueryParamsDto findParams);

    protected virtual ActionResult HandleAdminAuthorizationAsync()
    {
        if (!this.RoleService.IsAdmin(this.HttpContext.User))
        {
            return this.Unauthorized();
        }

        return this.Ok();
    }

    protected virtual void NormalizeQueryParamsForAdmin(QueryParamsDto queryParams)
    {
        if (this.RoleService.IsAdmin(this.User))
        {
            queryParams.Page = Math.Max(1, queryParams.Page);
            queryParams.PageSize = Math.Max(Constants.DEFAULT_PAGE_SIZE, queryParams.PageSize);
        }
        else
        {
            this.QueryService.NormaliseQueryParams(queryParams);
        }
    }
}
