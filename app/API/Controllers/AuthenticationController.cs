#region

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zora.Core;
using zora.Core.DTOs;
using zora.Core.Interfaces;

#endregion

namespace zora.API.Controllers;

[ApiController]
[Route("api/v1/authentication")]
[Produces("application/json")]
[Consumes("application/json")]
[ProducesResponseType<int>(StatusCodes.Status200OK)]
public sealed class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(IAuthenticationService authenticationService,
        ILogger<AuthenticationController> logger)
    {
        this._authenticationService = authenticationService;
        this._logger = logger;
    }

    [HttpPost("token")]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    public async Task<IActionResult> Authenticate([FromBody] LoginRequest login)
    {
        try
        {
            if (!this._authenticationService.IsValidLoginRequest(login))
            {
                return this.BadRequest();
            }

            if (!await this._authenticationService.AuthenticateUser(login))
            {
                string? ipAddress = this.HttpContext.Connection.RemoteIpAddress?.ToString();
                this._logger.LogInformation($"User {login.Username} ({ipAddress}) failed to authenticate.");
                return this.Unauthorized();
            }

            string jwt = this._authenticationService.GetJwt();
            this._logger.LogInformation($"User {login.Username} authenticated.");
            return this.Ok(new { token = jwt });
        }
        catch (Exception e)
        {
            this._logger.LogError(e, "Error authenticating user.");
            return this.StatusCode(500, Constants.Error500Message);
        }
    }

    [HttpGet("check")]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [Authorize]
    public IActionResult CheckAuthStatus()
    {
        try
        {
            return this.Ok(new { isAuthenticated = true });
        }
        catch (Exception e)
        {
            this._logger.LogError(e, "Error checking authentication status.");
            return this.StatusCode(500, Constants.Error500Message);
        }
    }
}
