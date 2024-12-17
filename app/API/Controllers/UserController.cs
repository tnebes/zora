#region

using System.ComponentModel;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zora.Core.Domain;
using zora.Core.DTOs;
using zora.Core.Interfaces;
using zora.Infrastructure.Helpers;

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
    [Description("Get all users")]
    [Authorize]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            if (!this._roleService.IsAdmin(this.User).Result)
            {
                return this.Unauthorized();
            }

            IEnumerable<User> users = (await this._userService.GetAllFullUsers()).ToList();
            int userCount = users.Count();
            return this.Ok(users.ToFullUserResponseDto(userCount, 1, userCount, this._mapper));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Failed to get users");
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
