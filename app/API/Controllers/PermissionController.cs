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

[Authorize]
[ApiController]
[Route("api/v1/permissions")]
[Produces("application/json")]
[Consumes("application/json")]
[Description("Permission API")]
public sealed class PermissionController : ControllerBase
{
    private readonly ILogger<PermissionController> _logger;
    private readonly IMapper _mapper;
    private readonly IPermissionService _permissionService;
    private readonly IQueryService _queryService;
    private readonly IRoleService _roleService;

    public PermissionController(
        IPermissionService permissionService,
        IRoleService roleService,
        IQueryService queryService,
        ILogger<PermissionController> logger,
        IMapper mapper)
    {
        this._permissionService = permissionService;
        this._roleService = roleService;
        this._queryService = queryService;
        this._logger = logger;
        this._mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PermissionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Permissions")]
    [Description("Get all permissions with pagination, searching and sorting support")]
    public async Task<ActionResult<PermissionResponseDto>> GetPermissions([FromQuery] QueryParamsDto queryParams)
    {
        try
        {
            if (!this._roleService.IsAdmin(this.HttpContext.User))
            {
                return this.Unauthorized();
            }

            this._queryService.NormaliseQueryParams(queryParams);

            Result<PermissionResponseDto> permissionResponseResult =
                await this._permissionService.GetPermissionsDtoAsync(queryParams);

            if (permissionResponseResult.IsFailed)
            {
                this._logger.LogError("Error getting permissions: {Error}", permissionResponseResult.Errors);
                return this.StatusCode(StatusCodes.Status500InternalServerError, permissionResponseResult.Errors);
            }

            return this.Ok(permissionResponseResult.Value);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting permissions");
            return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Permissions")]
    [Description("Get a permission by ID")]
    public async Task<ActionResult<PermissionDto>> GetById(long id)
    {
        try
        {
            if (!this._roleService.IsAdmin(this.HttpContext.User))
            {
                return this.Unauthorized();
            }

            Result<Permission> result = await this._permissionService.GetByIdAsync(id);

            if (result.IsFailed)
            {
                this._logger.LogError("Error getting permission: {Error}", result.Errors);
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
            this._logger.LogError(ex, "Error getting permission by id");
            return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status201Created)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Permissions")]
    [Description("Create a new permission")]
    public async Task<ActionResult<PermissionDto>> Create([FromBody] CreatePermissionDto dto)
    {
        try
        {
            if (!this._roleService.IsAdmin(this.HttpContext.User))
            {
                return this.Unauthorized();
            }

            Result<Permission> result = await this._permissionService.CreateAsync(
                dto.Name,
                dto.Description,
                dto.PermissionString);

            if (result.IsFailed)
            {
                this._logger.LogError("Error creating permission: {Error}", result.Errors);
                return this.StatusCode(StatusCodes.Status500InternalServerError, result.Errors);
            }

            PermissionDto permissionDto = this._mapper.Map<PermissionDto>(result.Value);
            return this.CreatedAtAction(
                nameof(this.GetById),
                new { id = permissionDto.Id },
                permissionDto);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error creating permission");
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
    public async Task<ActionResult<PermissionResponseDto>> FindPermissions([FromQuery] QueryParamsDto findParams)
    {
        try
        {
            if (this._roleService.IsAdmin(this.User))
            {
                findParams.Page = Math.Max(1, findParams.Page);
                findParams.PageSize = Math.Max(Constants.DEFAULT_PAGE_SIZE, findParams.PageSize);
            }
            else
            {
                this._queryService.NormaliseQueryParams(findParams);
            }

            Result<PermissionResponseDto> permissions = await this._permissionService.FindPermissionsAsync(findParams);

            if (permissions.IsFailed)
            {
                this._logger.LogWarning("Failed to find permissions with search term {SearchTerm}",
                    findParams.SearchTerm);
                return this.BadRequest();
            }

            return this.Ok(permissions.Value);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to find permissions");
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Permissions")]
    [Description("Delete a permission")]
    public async Task<ActionResult<long>> Delete(long id)
    {
        try
        {
            if (!this._roleService.IsAdmin(this.HttpContext.User))
            {
                return this.Unauthorized();
            }

            Result<bool> result = await this._permissionService.DeleteAsync(id);

            if (result.IsFailed)
            {
                this._logger.LogError("Error deleting permission: {Error}", result.Errors);
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
            this._logger.LogError(ex, "Error deleting permission");
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
    public async Task<ActionResult<PermissionDto>> Update(long id, [FromBody] UpdatePermissionDto dto)
    {
        try
        {
            if (!this._roleService.IsAdmin(this.HttpContext.User))
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
                this._logger.LogError("Error updating permission: {Error}", result.Errors);
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
            this._logger.LogError(ex, "Error updating permission");
            return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(PermissionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Permissions")]
    [Description("Search permissions by IDs")]
    public async Task<ActionResult<PermissionResponseDto>> SearchByIds([FromQuery] string permissionIds)
    {
        try
        {
            if (!this._roleService.IsAdmin(this.HttpContext.User))
            {
                return this.Unauthorized();
            }

            List<long> ids = permissionIds.Split(',')
                .Select(id => long.TryParse(id, out long value) ? value : 0)
                .Where(id => id > 0)
                .ToList();

            if (!ids.Any())
            {
                return this.BadRequest("Invalid permission IDs provided");
            }

            Result<PermissionResponseDto> result = await this._permissionService.GetPermissionsByIdsAsync(ids);

            if (result.IsFailed)
            {
                this._logger.LogError("Error searching permissions by IDs: {Error}", result.Errors);
                return this.StatusCode(StatusCodes.Status500InternalServerError, result.Errors);
            }

            return this.Ok(result.Value);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error searching permissions by IDs");
            return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
