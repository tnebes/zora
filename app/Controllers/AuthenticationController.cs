using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using zora.Common;

namespace zora.Controllers
{

    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthenticationController : ControllerBase
    {

        public class LoginRequest
        {
            public required string Username { get; set; }
            public required string Password { get; set; }
        }

        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(ILogger<AuthenticationController> logger)
        {
            this._logger = logger;
        }

        [HttpPost("token")]
        public IActionResult authenticate([FromBody] LoginRequest login)
        {
            try
            {
                if (login.Username != "tnebes" || login.Password != "letmeinside1")
                {
                    return Unauthorized("Invalid credentials");
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

                SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
                string jwt = tokenHandler.WriteToken(token);

                Log.Information($"User {login.Username} authenticated.");

                return Ok(jwt);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error authenticating user.");
                return StatusCode(500, Constants.Error500Message);
            }
        }

    }
}
