#region

using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zora.Core.DTOs;
using zora.Core.Interfaces;

#endregion

namespace zora.API.Controllers;

[ApiController]
[Route("api/v1/authorisation")]
[Produces("application/json")]
[Consumes("application/json")]
[ProducesResponseType<int>(StatusCodes.Status200OK)]
[Description("Authorisation API")]
public class AuthorisationController : ControllerBase, IZoraService
{
    private readonly IAuthorisationService _authorisationService;
    private readonly ILogger<AuthorisationController> _logger;

    public AuthorisationController(IAuthorisationService authorisationService, ILogger<AuthorisationController> logger)
    {
        this._authorisationService = authorisationService;
        this._logger = logger;
    }

    [Authorize]
    [HttpPost("is-authorised")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Tags("Authorisation")]
    [Description("Check if the user is authorised to perform the requested action on a given resource")]
    public async Task<IActionResult> IsAuthorised([FromBody] PermissionRequestDto? permissionRequest)
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

            return isAuthorised ? this.Ok() : this.Forbid();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "An error occurred while checking authorisation");
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
