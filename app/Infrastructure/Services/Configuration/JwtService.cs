#region

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using zora.Core;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.Interfaces;
using zora.Services.Configuration;

#endregion

namespace zora.Infrastructure.Services.Configuration;

[ServiceLifetime(ServiceLifetime.Scoped)]
public class JwtService : IJwtService, IZoraService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;
    private readonly ISecretsManagerService _secretsManagerService;

    public JwtService(ISecretsManagerService secretsManagerService, ILogger<JwtService> logger,
        IConfiguration configuration)
    {
        this._secretsManagerService = secretsManagerService;
        this._logger = logger;
        this._configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        List<Claim> claims = JwtService.GenerateUserClaims(user);

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        string signingKey = this._secretsManagerService.GetSecret(Constants.ISSUER_SIGNING_KEY);
        byte[] key = Encoding.UTF8.GetBytes(signingKey);

        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(Constants.TOKEN_EXPIRATION_HOURS),
            Issuer = this._configuration["Jwt:Issuer"],
            Audience = this._configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        this._logger.LogInformation("Generated token for user {Username}", user.Username);
        return tokenHandler.WriteToken(token);
    }

    public int GetTokenExpiration() => TimeSpan.FromHours(Constants.TOKEN_EXPIRATION_HOURS).Seconds;

    private static List<Claim> GenerateUserClaims(User user)
    {
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email)
        ];
        claims.AddRange(user.UserRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole.Role.Name)));

        return claims;
    }
}
