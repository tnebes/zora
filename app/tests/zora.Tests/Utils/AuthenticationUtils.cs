#region

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

#endregion

namespace zora.Tests.Utils;

public static class AuthenticationUtils
{
    private const string AdminId = "1";
    private const string RegularUserId = "2";
    private const string AnonymousUserId = "0";
    private const string TestSecretKey = "YourTestSecretKeyHereItShouldBeLongEnoughForHS256";
    private const string TestIssuer = "TestIssuer";
    private const string TestAudience = "TestAudience";

    public static IEnumerable<Claim> AdminClaims =>
    [
        new(ClaimTypes.Name, "admin"),
        new(ClaimTypes.NameIdentifier, AdminId),
        new(ClaimTypes.Email, "admin@zora.com"),
        new(ClaimTypes.Role, "Admin")
    ];

    public static IEnumerable<Claim> RegularUserClaims =>
    [
        new(ClaimTypes.Name, "user"),
        new(ClaimTypes.NameIdentifier, RegularUserId),
        new(ClaimTypes.Email, "user@zora.com"),
        new(ClaimTypes.Role, "User")
    ];

    public static IEnumerable<Claim> AnonymousUserClaims =>
    [
        new(ClaimTypes.Name, "anonymous"),
        new(ClaimTypes.NameIdentifier, AnonymousUserId),
        new(ClaimTypes.Role, "Anonymous")
    ];

    public static string GenerateJwtToken(IEnumerable<Claim> claims)
    {
        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(TestSecretKey));
        SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(
            TestIssuer,
            TestAudience,
            claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static TokenValidationParameters GetTestTokenValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = TestIssuer,
            ValidateAudience = true,
            ValidAudience = TestAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }
}
