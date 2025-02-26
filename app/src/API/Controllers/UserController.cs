#region

using System.ComponentModel;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;
using zora.Core.Interfaces.Services;
using Constants = zora.Core.Constants;

#endregion

namespace zora.API.Controllers;

/// <summary>
///     Controller for managing system users.
///     Provides CRUD operations for users with appropriate authorization checks.
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Produces("application/json")]
[Consumes("application/json")]
[Description("User API")]
public sealed class UserController : BaseCrudController<FullUserDto, CreateMinimumUserDto, UpdateUserDto,
    UserResponseDto<FullUserDto>, DynamicQueryUserParamsDto>
{
    private readonly IUserService _userService;

    public UserController(
        IRoleService roleService,
        IUserService userService,
        IQueryService queryService,
        ILogger<UserController> logger)
        : base(logger, roleService, queryService) =>
        this._userService = userService;

    /// <summary>
    ///     Retrieves a paginated list of users with support for filtering, sorting, and searching.
    /// </summary>
    /// <param name="queryParams">Query parameters including page number, page size, and search term</param>
    /// <returns>Paginated list of users wrapped in UserResponseDto</returns>
    [HttpGet]
    [ProducesResponseType(typeof(UserResponseDto<FullUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Users")]
    [Description("Retrieves a paginated list of users. Supports filtering, sorting, and searching.")]
    [Authorize]
    public override async Task<ActionResult<UserResponseDto<FullUserDto>>> Get([FromQuery] QueryParamsDto queryParams)
    {
        try
        {
            this.NormalizeQueryParamsForAdmin(queryParams);

            Result<UserResponseDto<FullUserDto>> users = await this._userService.GetDtoAsync(queryParams);

            if (users.IsFailed)
            {
                this.Logger.LogWarning("Failed to get users");
                return this.BadRequest();
            }

            return this.Ok(users.Value);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to get users");
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(UserResponseDto<FullUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Users")]
    [Description("Search for users with pagination, searching and sorting support")]
    [Authorize]
    public override async Task<ActionResult<UserResponseDto<FullUserDto>>> Search(
        [FromQuery] DynamicQueryUserParamsDto queryParams)
    {
        try
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

            Result<UserResponseDto<FullUserDto>> users = await this._userService.SearchAsync(queryParams);

            if (users.IsFailed)
            {
                this.Logger.LogWarning("Failed to search users with query params {@QueryParams}", queryParams);
                return this.BadRequest();
            }

            return this.Ok(users.Value);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to search users");
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    ///     Deletes a user by ID. Requires admin privileges or ownership of the account.
    /// </summary>
    /// <param name="id">ID of the user to delete</param>
    /// <returns>Boolean indicating success of the deletion operation</returns>
    [HttpDelete("{id:long}")]
    [ProducesResponseType<int>(StatusCodes.Status204NoContent)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Users")]
    [Description("Deletes a user by ID. Requires admin privileges or ownership of the account.")]
    [Authorize]
    public override async Task<ActionResult<bool>> Delete([FromRoute] long id)
    {
        try
        {
            Result<User> result = await this._userService.GetByIdAsync(id);

            if (result.IsFailed)
            {
                this.Logger.LogWarning("User with ID {UserId} not found", id);
                return this.NotFound();
            }

            User user = result.Value;

            if (this.RoleService.IsAdmin(this.User) || this._userService.ClaimIsUser(this.User, user.Username))
            {
                await this._userService.DeleteAsync(user.Id);
                this.Logger.LogInformation("User with ID {UserId} deleted", id);
                return this.NoContent();
            }

            this.Logger.LogWarning("User with ID {Id} is not authorised to delete user with ID {UserId}", id, user.Id);
            return this.Unauthorized();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to delete user with ID {UserId}", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    ///     Creates a new user in the system. Requires admin privileges.
    /// </summary>
    /// <param name="createMinimumUserDto">Data transfer object containing user details</param>
    /// <returns>Created user object</returns>
    [HttpPost]
    [ProducesResponseType(typeof(UserResponseDto<FullUserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType<int>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Users")]
    [Description("Creates a new user. Requires admin privileges.")]
    [Authorize]
    public override async Task<ActionResult<FullUserDto>> Create([FromBody] CreateMinimumUserDto createMinimumUserDto)
    {
        try
        {
            if (!this.RoleService.IsAdmin(this.User))
            {
                this.Logger.LogWarning("User {Username} is not authorised to create a new user",
                    this.User.Identity?.Name);
                return this.Unauthorized();
            }

            Result<User> result = await this._userService.CreateAsync(createMinimumUserDto);
            if (result.IsFailed)
            {
                this.Logger.LogWarning("Failed to create user");
                return this.BadRequest();
            }

            Result<User> user = await this._userService.GetByIdAsync(result.Value.Id);

            if (user.IsFailed)
            {
                this.Logger.LogWarning("Failed to get user with ID {UserId}", result.Value.Id);
                return this.BadRequest();
            }

            FullUserDto fullUserValue = this._userService.ToDto<FullUserDto>(user.Value);

            this.Logger.LogInformation("User with ID {UserId} created", fullUserValue.Id);
            return this.CreatedAtAction(nameof(this.Create), new { id = fullUserValue.Id }, fullUserValue);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to create user");
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    ///     Updates an existing user by ID. Requires admin privileges or ownership of the account.
    /// </summary>
    /// <param name="id">ID of the user to update</param>
    /// <param name="updateUserDto">Data transfer object containing updated user details</param>
    /// <returns>Updated user object</returns>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(UserResponseDto<FullUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Users")]
    [Description("Updates an existing user by ID. Requires admin privileges or ownership of the account.")]
    [Authorize]
    public override async Task<ActionResult<FullUserDto>> Update([FromRoute] long id,
        [FromBody] UpdateUserDto updateUserDto)
    {
        try
        {
            bool isAdmin = this.RoleService.IsAdmin(this.User);
            bool isSameUser = this._userService.ClaimIsUser(this.User, updateUserDto.Username);

            if (!isAdmin && !isSameUser)
            {
                this.Logger.LogWarning("User {Username} is not authorized to update user with ID {UserId}",
                    this.User.Identity?.Name, id);
                return this.Unauthorized();
            }

            Result<User> userResult = await this._userService.GetByIdAsync(id);
            if (userResult.IsFailed)
            {
                this.Logger.LogWarning("User with ID {UserId} not found", id);
                return this.NotFound();
            }

            User existingUser = userResult.Value;

            Result<User> updateResult = await this._userService.UpdateAsync(existingUser.Id, updateUserDto);
            if (updateResult.IsFailed)
            {
                this.Logger.LogWarning("Failed to update user {UserId}", id);
                return this.BadRequest();
            }

            FullUserDto updatedUserDto = this._userService.ToDto<FullUserDto>(updateResult.Value);
            this.Logger.LogInformation("User {UserId} updated successfully", id);
            return this.Ok(updatedUserDto);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to update user {UserId}", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("find")]
    [ProducesResponseType(typeof(UserResponseDto<FullUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Users")]
    [Description("Find users by partial matches of username, email, or role name")]
    [Authorize]
    public override async Task<ActionResult<UserResponseDto<FullUserDto>>> Find([FromQuery] QueryParamsDto findParams)
    {
        try
        {
            if (this.RoleService.IsAdmin(this.User))
            {
                findParams.Page = Math.Max(1, findParams.Page);
                findParams.PageSize = Math.Max(Constants.DEFAULT_PAGE_SIZE, findParams.PageSize);
            }
            else
            {
                this.QueryService.NormaliseQueryParams(findParams);
            }

            Result<UserResponseDto<FullUserDto>> users = await this._userService.FindAsync(findParams);

            if (users.IsFailed)
            {
                this.Logger.LogWarning("Failed to find users with search term {SearchTerm}", findParams.SearchTerm);
                return this.BadRequest();
            }

            return this.Ok(users.Value);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to find users");
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
