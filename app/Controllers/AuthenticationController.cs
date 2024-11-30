using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using zora.Common;

namespace zora.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthenticationController : ControllerBase
{
    public class LoginRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    [HttpPost("token")]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
    public IActionResult Authenticate([FromBody] LoginRequest login)
    {
        try
        {
            if (login.Username != "tnebes" || login.Password != "letmeinside1")
            {
                return this.Unauthorized("Invalid credentials");
            }

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = Encoding.UTF8.GetBytes(Constants.IssuerSigningKey);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256)
            };

            SecurityToken? token = tokenHandler.CreateToken(tokenDescriptor);
            string? jwt = tokenHandler.WriteToken(token);

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
