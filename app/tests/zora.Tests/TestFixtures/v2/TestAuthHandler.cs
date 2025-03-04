#region

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

#endregion

namespace zora.Tests.TestFixtures.v2;

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        List<Claim> claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, "testuser"),
            new(ClaimTypes.Email, "test@example.com"),
            new(ClaimTypes.Role, "User")
        };

        if (this.Request.Headers.Authorization.ToString().Contains("Admin"))
        {
            claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name, "admin"),
                new(ClaimTypes.Email, "admin@example.com"),
                new(ClaimTypes.Role, "Admin")
            };
        }
        else if (this.Request.Headers.Authorization.ToString().Contains("User:"))
        {
            string authHeader = this.Request.Headers.Authorization.ToString();
            string userId = authHeader.Split("User:")[1].Trim();

            claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, $"user{userId}"),
                new(ClaimTypes.Email, $"user{userId}@example.com"),
                new(ClaimTypes.Role, "User")
            };
        }

        ClaimsIdentity identity = new ClaimsIdentity(claims, "Test");
        ClaimsPrincipal principal = new ClaimsPrincipal(identity);
        AuthenticationTicket ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
