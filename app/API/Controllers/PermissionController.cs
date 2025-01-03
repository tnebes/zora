#region

using System.ComponentModel;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zora.Core;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/permissions")]
[Produces("application/json")]
[Consumes("application/json")]
[Description("Permission API")]
public sealed class PermissionController : BaseCrudController<PermissionDto, CreatePermissionDto, UpdatePermissionDto, PermissionResponseDto>
{
    private readonly IPermissionService _permissionService;
    private readonly IMapper _mapper;

    public PermissionController(
        IPermissionService permissionService,
        IRoleService roleService,
        IQueryService queryService,
        ILogger<PermissionController> logger,
        IMapper mapper)
        : base(logger, roleService, queryService)
    {
        this._permissionService = permissionService;
        this._mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PermissionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Permissions")]
    [Description("Get all permissions with pagination, searching and sorting support")]
    public override async Task<ActionResult<PermissionResponseDto>> Get([FromQuery] QueryParamsDto queryParams)
    {
        try
        {
            ActionResult authResult = this.HandleAdminAuthorizationAsync();
            if (authResult is UnauthorizedResult)
            {
                return this.Unauthorized();
            }

            this.QueryService.NormaliseQueryParams(queryParams);

            Result<PermissionResponseDto> permissionResponseResult =
                await this._permissionService.GetPermissionsDtoAsync(queryParams);

            if (permissionResponseResult.IsFailed)
            {
                this.Logger.LogError("Error getting permissions: {Error}", permissionResponseResult.Errors);
                return this.StatusCode(StatusCodes.Status500InternalServerError, permissionResponseResult.Errors);
            }

            return this.Ok(permissionResponseResult.Value);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error getting permissions");
            return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status201Created)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Permissions")]
    [Description("Create a new permission")]
    public override async Task<ActionResult<PermissionDto>> Create([FromBody] CreatePermissionDto dto)
    {
        try
        {
            ActionResult authResult = this.HandleAdminAuthorizationAsync();
            if (authResult is UnauthorizedResult)
            {
                return this.Unauthorized();
            }

            Result<Permission> result = await this._permissionService.CreateAsync(
                dto.Name,
                dto.Description,
                dto.PermissionString);

            if (result.IsFailed)
            {
                this.Logger.LogError("Error creating permission: {Error}", result.Errors);
                return this.StatusCode(StatusCodes.Status500InternalServerError, result.Errors);
            }

            PermissionDto permissionDto = this._mapper.Map<PermissionDto>(result.Value);
            return this.CreatedAtAction(
                nameof(this.Get),
                new { id = permissionDto.Id },
                permissionDto);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error creating permission");
            return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Permissions")]
    [Description("Update a permission")]
    public override async Task<ActionResult<PermissionDto>> Update(long id, [FromBody] UpdatePermissionDto dto)
    {
        try
        {
            ActionResult authResult = this.HandleAdminAuthorizationAsync();
            if (authResult is UnauthorizedResult)
            {
                return this.Unauthorized();
            }

            Result<Permission> result = await this._permissionService.UpdateAsync(
                id,
                dto.Name,
                dto.Description,
                dto.PermissionString);

            if (result.IsFailed)
            {
                this.Logger.LogError("Error updating permission: {Error}", result.Errors);
                return this.StatusCode(StatusCodes.Status500InternalServerError, result.Errors);
            }

            if (result.Value == null)
            {
                return this.NotFound();
            }

            return this.Ok(this._mapper.Map<PermissionDto>(result.Value));
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error updating permission");
            return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Permissions")]
    [Description("Delete a permission")]
    public override async Task<ActionResult<bool>> Delete(long id)
    {
        try
        {
            ActionResult authResult = this.HandleAdminAuthorizationAsync();
            if (authResult is UnauthorizedResult)
            {
                return this.Unauthorized();
            }

            Result<bool> result = await this._permissionService.DeleteAsync(id);

            if (result.IsFailed)
            {
                this.Logger.LogError("Error deleting permission: {Error}", result.Errors);
                return this.StatusCode(StatusCodes.Status500InternalServerError, result.Errors);
            }

            if (!result.Value)
            {
                return this.NotFound();
            }

            return this.NoContent();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error deleting permission");
            return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpGet("find")]
    [ProducesResponseType(typeof(PermissionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Permissions")]
    [Description("Find permissions by partial matches of name or description")]
    public override async Task<ActionResult<PermissionResponseDto>> Find([FromQuery] QueryParamsDto findParams)
    {
        try
        {
            this.NormalizeQueryParamsForAdmin(findParams);

            Result<PermissionResponseDto> permissions = await this._permissionService.FindPermissionsAsync(findParams);

            if (permissions.IsFailed)
            {
                this.Logger.LogWarning("Failed to find permissions with search term {SearchTerm}",
                    findParams.SearchTerm);
                return this.BadRequest();
            }

            return this.Ok(permissions.Value);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to find permissions");
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
