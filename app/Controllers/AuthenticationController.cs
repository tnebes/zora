using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using zora.Core;
using zora.Core.DTOs;
using zora.Core.Interfaces;

namespace zora.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
[ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
[ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<int>(StatusCodes.Status200OK)]
public class AuthenticationController : ControllerBase
{

    private readonly IAuthenticationService _authenticationService;

    public AuthenticationController(IAuthenticationService authenticationService)
    {
        this._authenticationService = authenticationService;
    }

    [HttpPost("token")]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    public IActionResult Authenticate([FromBody] LoginRequest login)
    {
        try
        {
            if (!this._authenticationService.IsValidLoginRequest(login))
            {
                return this.BadRequest();
            }

            if (this._authenticationService.AuthenticateUser(login))
            {
                string? ipAddress = this.HttpContext.Connection.RemoteIpAddress?.ToString();
                Log.Information($"User {login.Username} ({ipAddress}) failed to authenticate.");
                return this.Unauthorized();
            }

            string jwt = this._authenticationService.GetJwt();
            Log.Information($"User {login.Username} authenticated.");
            return this.Ok(new { token = jwt });
        }
        catch (Exception e)
        {
            Log.Error(e, "Error authenticating user.");
            return this.StatusCode(500, Constants.Error500Message);
        }
    }
}
