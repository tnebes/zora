#region

using System.ComponentModel;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zora.Core;
using zora.Core.Domain;
using zora.Core.DTOs;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.API.Controllers;

[ApiController]
[Route("api/v1/roles")]
[Produces("application/json")]
[Consumes("application/json")]
[Description("Role API")]
public sealed class RoleController : ControllerBase, IZoraService
{
    private readonly ILogger<RoleController> _logger;
    private readonly IMapper _mapper;
    private readonly IQueryService _queryService;
    private readonly IRoleService _roleService;

    public RoleController(
        IRoleService roleService,
        IQueryService queryService,
        IMapper mapper,
        ILogger<RoleController> logger)
    {
        this._roleService = roleService;
        this._queryService = queryService;
        this._mapper = mapper;
        this._logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(RoleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Roles")]
    [Description("Get all roles with pagination, searching and sorting support")]
    [Authorize]
    public async Task<IActionResult> GetRoles([FromQuery] QueryParamsDto queryParams)
    {
        try
        {
            if (!this._roleService.IsAdmin(this.HttpContext.User))
            {
                return this.Unauthorized();
            }

            this._queryService.NormaliseQueryParams(queryParams);

            Result<RoleResponseDto> roleResponseResult = await this._roleService.GetRolesDtoAsync(queryParams);

            if (roleResponseResult.IsFailed)
            {
                this._logger.LogError("Error getting roles: {Error}", roleResponseResult.Errors);
                return this.StatusCode(StatusCodes.Status500InternalServerError, roleResponseResult.Errors);
            }

            return this.Ok(roleResponseResult.Value);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting roles");
            return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Roles")]
    [Description("Create a new role")]
    [Authorize]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto roleDto)
    {
        try
        {
            if (!this._roleService.IsAdmin(this.HttpContext.User))
            {
                return this.Unauthorized();
            }

            Result<Role> roleResult = await this._roleService.CreateRoleAsync(roleDto);

            if (roleResult.IsFailed)
            {
                this._logger.LogError("Error creating role: {Error}", roleResult.Errors);
                return this.StatusCode(StatusCodes.Status500InternalServerError, roleResult.Errors);
            }

            Role role = roleResult.Value;

            return this.CreatedAtAction(nameof(RoleController.GetRoles), new { id = role.Id }, role);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error creating role");
            return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Roles")]
    [Description("Update an existing role")]
    [Authorize]
    public async Task<IActionResult> UpdateRole(long id, [FromBody] UpdateRoleDto roleDto)
    {
        try
        {
            if (!this._roleService.IsAdmin(this.HttpContext.User))
            {
                return this.Unauthorized();
            }

            Result<Role> role = await this._roleService.UpdateRoleAsync(id, roleDto);
            if (role.IsFailed)
            {
                this._logger.LogError("Error updating role: {Error}", role.Errors);
                return this.NotFound();
            }

            return this.Ok(role);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error updating role");
            return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Roles")]
    [Description("Delete a role")]
    [Authorize]
    public async Task<IActionResult> DeleteRole(long id)
    {
        try
        {
            if (!this._roleService.IsAdmin(this.HttpContext.User))
            {
                return this.Unauthorized();
            }

            bool success = await this._roleService.DeleteRoleAsync(id);
            if (!success)
            {
                return this.NotFound();
            }

            return this.NoContent();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error deleting role");
            return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpGet("find")]
    [ProducesResponseType(typeof(RoleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Roles")]
    [Description("Find roles by partial matches of name or description")]
    [Authorize]
    public async Task<IActionResult> FindRoles([FromQuery] QueryParamsDto findParams)
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

            Result<RoleResponseDto> roles = await this._roleService.FindRolesAsync(findParams);

            if (roles.IsFailed)
            {
                this._logger.LogWarning("Failed to find roles with search term {SearchTerm}", findParams.SearchTerm);
                return this.BadRequest();
            }

            return this.Ok(roles.Value);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to find roles");
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
