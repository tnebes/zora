#region

using System.ComponentModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zora.Core.Interfaces;

#endregion

namespace zora.API.Controllers;

[ApiController]
[Route("api/v1/control-panel")]
[Produces("application/json")]
[Consumes("application/json")]
[ProducesResponseType<int>(StatusCodes.Status200OK)]
[Description("Control Panel API")]
public class ControlPanelController : ControllerBase, IZoraService
{
    private readonly ILogger<AuthorisationController> _logger;

    public ControlPanelController(ILogger<AuthorisationController> logger) => this._logger = logger;

    [Authorize]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Tags("Control Panel")]
    [Description("Get the control panel")]
    public async Task<IActionResult> GetControlPanel([FromBody] ClaimsPrincipal user)
    {
        try
        {
            return this.Ok();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "An error occurred while getting the control panel");
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
