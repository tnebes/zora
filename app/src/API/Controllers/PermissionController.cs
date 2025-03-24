#region

using System.ComponentModel;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zora.Core;
using zora.Core.Domain;
using zora.Core.DTOs.Permissions;
using zora.Core.DTOs.Requests;
using zora.Core.Enums;
using zora.Core.Interfaces.Services;
using zora.Core.Utilities;

#endregion

namespace zora.API.Controllers;

/// <summary>
///     Controller for managing system permissions.
///     Provides CRUD operations for permissions with admin authorization checks.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/permissions")]
[Produces("application/json")]
[Consumes("application/json")]
[Description("Permission API")]
public sealed class PermissionController : BaseCrudController<PermissionDto, CreatePermissionDto, UpdatePermissionDto,
    PermissionDto,
    PermissionResponseDto, DynamicQueryPermissionParamsDto>
{
    private readonly IMapper _mapper;
    private readonly IPermissionService _permissionService;

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

    /// <summary>
    ///     Retrieves a paginated list of permissions with support for filtering, sorting, and searching.
    /// </summary>
    /// <param name="queryParams">Query parameters including page number, page size, and search term</param>
    /// <returns>Paginated list of permissions wrapped in PermissionResponseDto</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PermissionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Permissions")]
    [Description("Retrieves a paginated list of permissions. Supports filtering, sorting, and searching.")]
    public override async Task<ActionResult<PermissionResponseDto>> Get([FromQuery] QueryParamsDto queryParams)
    {
        try
        {
            ActionResult authResult = this.HandleAdminAuthorization();
            if (authResult is UnauthorizedResult)
            {
                this.LogUnauthorisedAccess(this.HttpContext.User);
                return this.Unauthorized();
            }

            this.QueryService.NormaliseQueryParams(queryParams);

            Result<PermissionResponseDto> permissionResponseResult =
                await this._permissionService.GetDtoAsync(queryParams);

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
            return this.StatusCode(StatusCodes.Status500InternalServerError, Constants.ERROR_500_MESSAGE);
        }
    }

    /// <summary>
    ///     Creates a new permission in the system. Requires admin privileges.
    /// </summary>
    /// <param name="dto">Data transfer object containing permission details</param>
    /// <returns>Created permission object</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status201Created)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Permissions")]
    [Description("Creates a new permission. Requires admin privileges.")]
    public override async Task<ActionResult<PermissionDto>> Create([FromBody] CreatePermissionDto dto)
    {
        try
        {
            ActionResult authResult = this.HandleAdminAuthorization();
            if (authResult is UnauthorizedResult)
            {
                this.LogUnauthorisedAccess(this.HttpContext.User);
                return this.Unauthorized();
            }

            Result<Permission> result = await this._permissionService.CreateAsync(dto);

            if (result.IsFailed)
            {
                if (ResultUtilities.IsValidationError(result, ErrorType.ValidationError))
                {
                    this.Logger.LogError("Error creating permission: {Error}", result.Errors);
                    return this.BadRequest();
                }

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
            return this.StatusCode(StatusCodes.Status500InternalServerError, Constants.ERROR_500_MESSAGE);
        }
    }

    /// <summary>
    ///     Updates an existing permission by ID. Requires admin privileges.
    /// </summary>
    /// <param name="id">ID of the permission to update</param>
    /// <param name="dto">Data transfer object containing updated permission details</param>
    /// <returns>Updated permission object</returns>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Permissions")]
    [Description("Updates an existing permission by ID. Requires admin privileges.")]
    public override async Task<ActionResult<PermissionDto>> Update(long id, [FromBody] UpdatePermissionDto dto)
    {
        try
        {
            ActionResult authResult = this.HandleAdminAuthorization();
            if (authResult is UnauthorizedResult)
            {
                this.LogUnauthorisedAccess(this.HttpContext.User);
                return this.Unauthorized();
            }

            Result<Permission> result = await this._permissionService.UpdateAsync(id, dto);

            if (result.IsFailed)
            {
                if (ResultUtilities.IsValidationError(result, ErrorType.NotFound))
                {
                    this.Logger.LogError("Error updating permission: {Error}", result.Errors);
                    return this.NotFound();
                }

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
            return this.StatusCode(StatusCodes.Status500InternalServerError, Constants.ERROR_500_MESSAGE);
        }
    }

    /// <summary>
    ///     Deletes a permission by ID. Requires admin privileges.
    /// </summary>
    /// <param name="id">ID of the permission to delete</param>
    /// <returns>Boolean indicating success of the deletion operation</returns>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Permissions")]
    [Description("Deletes a permission by ID. Requires admin privileges.")]
    public override async Task<ActionResult<bool>> Delete(long id)
    {
        try
        {
            ActionResult authResult = this.HandleAdminAuthorization();
            if (authResult is UnauthorizedResult)
            {
                this.LogUnauthorisedAccess(this.HttpContext.User);
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
            return this.StatusCode(StatusCodes.Status500InternalServerError, Constants.ERROR_500_MESSAGE);
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

            Result<PermissionResponseDto> permissions = await this._permissionService.FindAsync(findParams);

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
            return this.StatusCode(StatusCodes.Status500InternalServerError, Constants.ERROR_500_MESSAGE);
        }
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(PermissionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Permissions")]
    [Description("Search permissions by matches of name, description, permission string, role, and work item")]
    public override async Task<ActionResult<PermissionResponseDto>> Search(
        [FromQuery] DynamicQueryPermissionParamsDto searchParams)
    {
        try
        {
            ActionResult authResult = this.HandleAdminAuthorization();
            if (authResult is UnauthorizedResult)
            {
                this.LogUnauthorisedAccess(this.HttpContext.User);
                return this.Unauthorized();
            }

            Result<PermissionResponseDto> result = await this._permissionService.SearchAsync(searchParams);
            return result.IsFailed ? this.BadRequest(result.Errors) : this.Ok(result.Value);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error searching permissions");
            return this.StatusCode(StatusCodes.Status500InternalServerError, Constants.ERROR_500_MESSAGE);
        }
    }
}
