#region

using System.ComponentModel;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;
using zora.Core.Interfaces;
using Constants = zora.Core.Constants;

#endregion

namespace zora.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Produces("application/json")]
[Consumes("application/json")]
[Description("User API")]
public class UserController : ControllerBase, IZoraService
{
    private readonly IAuthorisationService _authorisationService;
    private readonly ILogger<UserController> _logger;
    private readonly IMapper _mapper;
    private readonly IRoleService _roleService;
    private readonly IUserService _userService;

    public UserController(
        IAuthorisationService authorisationService,
        IRoleService roleService,
        IUserService userService,
        ILogger<UserController> logger,
        IMapper mapper)
    {
        this._authorisationService = authorisationService;
        this._roleService = roleService;
        this._userService = userService;
        this._logger = logger;
        this._mapper = mapper;
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
            if (!this._roleService.IsAdmin(this.User).Result)
            {
                // TODO FIXME this could have been handled by the validation in the DTO
                // but since we are relying on this stupid hack so that the admin
                // can see all users, we need to do this here
                // very bad design
                queryParams.Page = Math.Max(1, queryParams.Page);
                queryParams.PageSize = Math.Clamp(queryParams.PageSize, Constants.DEFAULT_PAGE_SIZE,
                    Constants.MAX_RESULTS_PER_PAGE);
            }
            else
            {
                queryParams.Page = Math.Max(1, queryParams.Page);
                queryParams.PageSize = Math.Max(Constants.DEFAULT_PAGE_SIZE, queryParams.PageSize);
            }

            UserResponseDto<FullUserDto> users = await this._userService.GetUsersDtoAsync(queryParams);
            return this.Ok(users);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to get users");
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
