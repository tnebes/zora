#region

using System.ComponentModel;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;
using zora.Core.DTOs.Responses.Interface;
using zora.Core.Interfaces.Services;
using Constants = zora.Core.Constants;

#endregion

namespace zora.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Produces("application/json")]
[Consumes("application/json")]
[Description("User API")]
public sealed class UserController : ControllerBase, IZoraService
{
    private readonly ILogger<UserController> _logger;
    private readonly IQueryService _queryService;
    private readonly IRoleService _roleService;
    private readonly IUserService _userService;

    public UserController(
        IRoleService roleService,
        IUserService userService,
        IQueryService queryService,
        ILogger<UserController> logger,
        IMapper mapper)
    {
        this._roleService = roleService;
        this._userService = userService;
        this._queryService = queryService;
        this._logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(UserResponseDto<FullUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Users")]
    [Description("Get all users with pagination, searching and sorting support")]
    [Authorize]
    public async Task<IActionResult> GetUsers([FromQuery] QueryParamsDto queryParams)
    {
        try
        {
            // TODO FIXME this could have been handled by the validation in the DTO
            // but since we are relying on this stupid hack so that the admin
            // can see all users, we need to do this here
            // very bad design
            if (this._roleService.IsAdmin(this.User))
            {
                queryParams.Page = Math.Max(1, queryParams.Page);
                queryParams.PageSize = Math.Max(Constants.DEFAULT_PAGE_SIZE, queryParams.PageSize);
            }
            else
            {
                this._queryService.NormaliseQueryParams(queryParams);
            }

            Result<UserResponseDto<FullUserDto>> users = await this._userService.GetUsersDtoAsync(queryParams);

            if (users.IsFailed)
            {
                this._logger.LogWarning("Failed to get users");
                return this.BadRequest();
            }

            return this.Ok(users.Value);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to get users");
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
    public async Task<IActionResult> SearchUsers([FromQuery] DynamicQueryUserParamsDto queryParams)
    {
        try
        {
            if (this._roleService.IsAdmin(this.User))
            {
                queryParams.Page = Math.Max(1, queryParams.Page);
                queryParams.PageSize = Math.Max(Constants.DEFAULT_PAGE_SIZE, queryParams.PageSize);
            }
            else
            {
                this._queryService.NormaliseQueryParams(queryParams);
            }

            Result<UserResponseDto<FullUserDto>> users = await this._userService.SearchUsersAsync(queryParams);

            if (users.IsFailed)
            {
                this._logger.LogWarning("Failed to search users with query params {@QueryParams}", queryParams);
                return this.BadRequest();
            }

            return this.Ok(users.Value);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to search users");
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType<int>(StatusCodes.Status204NoContent)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Users")]
    [Description("Delete a user by ID")]
    [Authorize]
    public async Task<IActionResult> DeleteUser([FromRoute] long id)
    {
        try
        {
            Result<User> result = await this._userService.GetUserByIdAsync(id);

            if (result.IsFailed)
            {
                this._logger.LogWarning("User with ID {UserId} not found", id);
                return this.NotFound();
            }

            User user = result.Value;

            if (this._roleService.IsAdmin(this.User) || this._userService.ClaimIsUser(this.User, user.Username))
            {
                await this._userService.DeleteUserAsync(user);
                this._logger.LogInformation("User with ID {UserId} deleted", id);
                return this.NoContent();
            }

            this._logger.LogWarning("User with ID {Id} is not authorised to delete user with ID {UserId}", id,
                user.Id);
            return this.Unauthorized();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to delete user with ID {UserId}", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserResponseDto<FullUserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType<int>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Users")]
    [Description("Create a new user")]
    [Authorize]
    public async Task<IActionResult> CreateUser([FromBody] CreateMinimumUserDto createMinimumUserDto)
    {
        try
        {
            if (this._roleService.IsAdmin(this.User))
            {
                Result<User> result = await this._userService.CreateAsync(createMinimumUserDto);
                if (result.IsFailed)
                {
                    this._logger.LogWarning("Failed to create user");
                    return this.BadRequest();
                }

                Result<FullUserDto> fullUser = await this._userService.GetUserDtoByIdAsync(result.Value.Id);

                if (fullUser.IsFailed)
                {
                    this._logger.LogWarning("Failed to get user with ID {UserId}", result.Value.Id);
                    return this.BadRequest();
                }

                FullUserDto fullUserValue = fullUser.Value;

                this._logger.LogInformation("User with ID {UserId} created", fullUserValue.Id);
                return this.CreatedAtAction(nameof(this.CreateUser), new { id = fullUserValue.Id }, fullUserValue);
            }

            this._logger.LogWarning("User {Username} is not authorised to create a new user", this.User.Identity?.Name);
            return this.Unauthorized();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to create user");
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserResponseDto<FullUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Users")]
    [Description("Update a user by ID")]
    [Authorize]
    public async Task<IActionResult> UpdateUser([FromRoute] long id, [FromBody] UpdateUserDto updateUserDto)
    {
        try
        {
            Result<User> result = await this._userService.GetUserByIdAsync(id);

            if (result.IsFailed)
            {
                this._logger.LogWarning("User with ID {UserId} not found", id);
                return this.NotFound();
            }

            User user = result.Value;

            if (this._roleService.IsAdmin(this.User) || this._userService.ClaimIsUser(this.User, user.Username))
            {
                Result<User> updatedUser = await this._userService.UpdateUserAsync(user, updateUserDto);
                if (updatedUser.IsFailed)
                {
                    this._logger.LogWarning("Failed to update user with ID {UserId}", id);
                    return this.BadRequest();
                }

                FullUserDto fullUser = this._userService.ToDto<FullUserDto>(updatedUser.Value);
                this._logger.LogInformation("User with ID {UserId} updated", id);
                return this.Ok(fullUser);
            }

            this._logger.LogWarning("User with ID {Id} is not authorised to update user with ID {UserId}", id,
                user.Id);
            return this.Unauthorized();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to update user with ID {UserId}", id);
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
    public async Task<IActionResult> FindUsers([FromQuery] QueryParamsDto findParams)
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

            Result<UserResponseDto<FullUserDto>> users = await this._userService.FindUsersAsync(findParams);

            if (users.IsFailed)
            {
                this._logger.LogWarning("Failed to find users with search term {SearchTerm}", findParams.SearchTerm);
                return this.BadRequest();
            }

            return this.Ok(users.Value);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to find users");
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
