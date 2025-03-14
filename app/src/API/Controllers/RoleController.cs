#region

using System.ComponentModel;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Roles;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.API.Controllers;

/// <summary>
///     Controller for managing system roles.
///     Provides CRUD operations for roles with admin authorization checks.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/roles")]
[Produces("application/json")]
[Consumes("application/json")]
[Description("Role API")]
public sealed class RoleController : BaseCrudController<FullRoleDto, CreateRoleDto, UpdateRoleDto, RoleResponseDto,
    DynamicQueryRoleParamsDto>
{
    private readonly IRoleService _roleService;

    public RoleController(
        IRoleService roleService,
        IQueryService queryService,
        ILogger<RoleController> logger)
        : base(logger, roleService, queryService) =>
        this._roleService = roleService;

    /// <summary>
    ///     Retrieves a paginated list of roles with support for filtering, sorting, and searching.
    /// </summary>
    /// <param name="queryParams">Query parameters including page number, page size, and search term</param>
    /// <returns>Paginated list of roles wrapped in RoleResponseDto</returns>
    [HttpGet]
    [ProducesResponseType(typeof(RoleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Roles")]
    [Description("Retrieves a paginated list of roles. Supports filtering, sorting, and searching.")]
    public override async Task<ActionResult<RoleResponseDto>> Get([FromQuery] QueryParamsDto queryParams)
    {
        try
        {
            ActionResult authResult = this.HandleAdminAuthorization();
            if (authResult is UnauthorizedResult)
            {
                return this.Unauthorized();
            }

            this.NormalizeQueryParamsForAdmin(queryParams);

            Result<RoleResponseDto> roleResponseResult = await this.RoleService.GetDtoAsync(queryParams);

            if (roleResponseResult.IsFailed)
            {
                this.Logger.LogError("Error getting roles: {Error}", roleResponseResult.Errors);
                return this.StatusCode(StatusCodes.Status500InternalServerError, roleResponseResult.Errors);
            }

            return this.Ok(roleResponseResult.Value);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error getting roles");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    ///     Creates a new role in the system. Requires admin privileges.
    /// </summary>
    /// <param name="roleDto">Data transfer object containing role details</param>
    /// <returns>Created role object</returns>
    [HttpPost]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Roles")]
    [Description("Creates a new role. Requires admin privileges.")]
    public override async Task<ActionResult<FullRoleDto>> Create([FromBody] CreateRoleDto roleDto)
    {
        try
        {
            if (!this.RoleService.IsAdmin(this.HttpContext.User))
            {
                return this.Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(roleDto.Name))
            {
                return this.BadRequest("Role name cannot be empty or whitespace.");
            }

            Result<Role> roleResult = await this.RoleService.CreateAsync(roleDto);

            if (roleResult.IsFailed)
            {
                this.Logger.LogError("Error creating role: {Error}", roleResult.Errors);
                return this.StatusCode(StatusCodes.Status500InternalServerError, roleResult.Errors);
            }

            FullRoleDto fullRoleDto = this.RoleService.MapToFullDto(roleResult.Value);

            return this.CreatedAtAction(nameof(this.Get), new { id = fullRoleDto.Id }, fullRoleDto);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error creating role");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    ///     Updates an existing role by ID. Requires admin privileges.
    /// </summary>
    /// <param name="id">ID of the role to update</param>
    /// <param name="roleDto">Data transfer object containing updated role details</param>
    /// <returns>Updated role object</returns>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Roles")]
    [Description("Updates an existing role by ID. Requires admin privileges.")]
    public override async Task<ActionResult<FullRoleDto>> Update(long id, [FromBody] UpdateRoleDto roleDto)
    {
        try
        {
            if (!this.RoleService.IsAdmin(this.HttpContext.User))
            {
                return this.Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(roleDto.Name))
            {
                return this.BadRequest("Role name cannot be empty or whitespace.");
            }

            Result<Role> roleResult = await this.RoleService.UpdateAsync(id, roleDto);
            if (roleResult.IsFailed)
            {
                this.Logger.LogError("Error updating role: {Error}", roleResult.Errors);
                return this.NotFound();
            }

            FullRoleDto fullRoleDto = this.RoleService.MapToFullDto(roleResult.Value);

            return this.Ok(fullRoleDto);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error updating role");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    ///     Deletes a role by ID. Requires admin privileges.
    /// </summary>
    /// <param name="id">ID of the role to delete</param>
    /// <returns>Boolean indicating success of the deletion operation</returns>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Roles")]
    [Description("Deletes a role by ID. Requires admin privileges.")]
    public override async Task<ActionResult<bool>> Delete(long id)
    {
        try
        {
            if (!this.RoleService.IsAdmin(this.HttpContext.User))
            {
                return this.Unauthorized();
            }

            bool success = await this.RoleService.DeleteAsync(id);
            if (!success)
            {
                return this.NotFound();
            }

            return this.NoContent();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error deleting role");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    [HttpGet("find")]
    [ProducesResponseType(typeof(RoleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Roles")]
    [Description("Find roles by partial matches of name or description")]
    public override async Task<ActionResult<RoleResponseDto>> Find([FromQuery] QueryParamsDto findParams)
    {
        try
        {
            if (this.RoleService.IsAdmin(this.User))
            {
                this.QueryService.NormaliseQueryParamsForAdmin(findParams);
            }
            else
            {
                this.QueryService.NormaliseQueryParams(findParams);
            }

            Result<RoleResponseDto> roles = await this.RoleService.FindAsync(findParams);

            if (roles.IsFailed)
            {
                this.Logger.LogWarning("Failed to find roles with search term {SearchTerm}", findParams.SearchTerm);
                return this.BadRequest();
            }

            return this.Ok(roles.Value);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to find roles");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(RoleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Roles")]
    [Description("Search roles with pagination, searching and sorting support")]
    [Authorize]
    public override async Task<ActionResult<RoleResponseDto>> Search([FromQuery] DynamicQueryRoleParamsDto searchParams)
    {
        try
        {
            if (this.RoleService.IsAdmin(this.User))
            {
                this.QueryService.NormaliseQueryParamsForAdmin(searchParams);
            }
            else
            {
                this.QueryService.NormaliseQueryParams(searchParams);
            }

            Result<RoleResponseDto> result = await this.RoleService.SearchAsync(searchParams);

            if (result.IsFailed)
            {
                this.Logger.LogWarning("Failed to search roles with query params {SearchParams}", searchParams);
                return this.BadRequest(result.Errors.FirstOrDefault()?.Message);
            }

            return this.Ok(result.Value);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error searching roles");
            return this.StatusCode(500, "An unexpected error occurred");
        }
    }
}
