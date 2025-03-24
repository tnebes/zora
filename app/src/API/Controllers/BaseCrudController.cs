#region

using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using zora.API.Interfaces;
using zora.Core;
using zora.Core.DTOs.Requests;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.API.Controllers;

/// <summary>
///     Base controller providing common CRUD operations.
///     Implements basic functionality for create, read, update, and delete operations.
/// </summary>
public abstract class BaseCrudController<TEntity, TCreateDto, TUpdateDto, TReadDto, TResponseDto, TDynamicQueryDto> :
    ControllerBase,
    ICrudController<TEntity, TCreateDto, TUpdateDto, TReadDto, TResponseDto, TDynamicQueryDto>
    where TDynamicQueryDto : DynamicQueryParamsDto
{
    protected readonly
        ILogger<BaseCrudController<TEntity, TCreateDto, TUpdateDto, TReadDto, TResponseDto, TDynamicQueryDto>>
        Logger;

    protected readonly IQueryService QueryService;
    protected readonly IRoleService RoleService;

    protected BaseCrudController(
        ILogger<BaseCrudController<TEntity, TCreateDto, TUpdateDto, TReadDto, TResponseDto, TDynamicQueryDto>> logger,
        IRoleService roleService,
        IQueryService queryService)
    {
        this.Logger = logger;
        this.RoleService = roleService;
        this.QueryService = queryService;
    }

    public abstract Task<ActionResult<TResponseDto>> Get([FromQuery] QueryParamsDto queryParams);
    public abstract Task<ActionResult<TReadDto>> Create([FromBody] TCreateDto createDto);
    public abstract Task<ActionResult<TReadDto>> Update(long id, [FromBody] TUpdateDto updateDto);
    public abstract Task<ActionResult<bool>> Delete(long id);
    public abstract Task<ActionResult<TResponseDto>> Find(QueryParamsDto findParams);

    public abstract Task<ActionResult<TResponseDto>> Search(TDynamicQueryDto searchParams);

    protected virtual ActionResult HandleAdminAuthorization()
    {
        if (!this.RoleService.IsAdmin(this.HttpContext.User))
        {
            this.LogUnauthorisedAccess(this.HttpContext.User);
            return this.Unauthorized();
        }

        return this.Ok();
    }

    protected virtual void NormalizeQueryParamsForAdmin(QueryParamsDto queryParams)
    {
        if (this.RoleService.IsAdmin(this.User))
        {
            queryParams.Page = Math.Max(1, queryParams.Page);
            queryParams.PageSize = queryParams.PageSize <= 0 ? Constants.DEFAULT_PAGE_SIZE : queryParams.PageSize;
        }
        else
        {
            this.QueryService.NormaliseQueryParams(queryParams);
        }
    }

    protected void LogUnauthorisedAccess(ClaimsPrincipal user)
    {
        string ipAddress = this.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
        string userAgent = this.HttpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown User-Agent";
        string endpoint = this.RouteData.Values["action"]?.ToString() ?? string.Empty;
        this.Logger.LogWarning("Unauthorized access attempt from {IpAddress} to {Endpoint}. User-Agent: {UserAgent}",
            ipAddress, endpoint, userAgent);
    }
}
