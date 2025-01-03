#region

using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zora.Core.DTOs;
using zora.Core.DTOs.Requests;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.API.Controllers;

[ApiController]
[Route("api/v1/authorisation")]
[Produces("application/json")]
[Consumes("application/json")]
[Description("Authorisation API")]
public sealed class AuthorisationController : ControllerBase, IZoraService
{
    private readonly IAuthorisationService _authorisationService;
    private readonly ILogger<AuthorisationController> _logger;
    private readonly IRoleService _roleService;

    public AuthorisationController(IAuthorisationService authorisationService,
        IPermissionService permissionService,
        IUserRoleService userRoleService,
        IRoleService roleService,
        ILogger<AuthorisationController> logger)
    {
        this._authorisationService = authorisationService;
        this._roleService = roleService;
        this._logger = logger;
    }

    [Authorize]
    [HttpPost("is-authorised")]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status403Forbidden)]
    [Tags("Authorisation")]
    [Description("Check if the user is authorised to perform the requested action on a given resource")]
    public async Task<ActionResult<bool>> IsAuthorised([FromBody] PermissionRequestDto permissionRequest)
    {
        try
        {
            ValidationResult validationResult =
                this._authorisationService.ValidateRequestAndClaims(permissionRequest, this.User);

            if (!validationResult.IsValid)
            {
                return this.StatusCode(validationResult.StatusCode, validationResult.ErrorMessage);
            }

            bool isAuthorised = await this._authorisationService.IsAuthorisedAsync(permissionRequest);
            return isAuthorised ? this.Ok(true) : this.Forbid();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "An error occurred while checking authorisation");
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [Authorize]
    [HttpGet("is-admin")]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status403Forbidden)]
    [Tags("Authorisation")]
    [Description("Check if the user is admin")]
    public ActionResult<bool> IsAdmin()
    {
        try
        {
            bool isAdmin = this._roleService.IsAdmin(this.HttpContext.User);
            return isAdmin ? this.Ok(true) : this.Forbid();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "An error occurred while checking if the user is admin");
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
