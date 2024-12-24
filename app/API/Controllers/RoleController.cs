#region

using System.ComponentModel;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

            RoleResponseDto roleResponse = await this._roleService.GetRolesDtoAsync(queryParams);
            return this.Ok(roleResponse);
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

            var role = await this._roleService.CreateRoleAsync(roleDto);
            return this.CreatedAtAction(nameof(GetRoles), new { id = role.Id }, role);
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

            var role = await this._roleService.UpdateRoleAsync(id, roleDto);
            if (role == null)
            {
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

            var success = await this._roleService.DeleteRoleAsync(id);
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
}
