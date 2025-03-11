#region

using System.Security.Claims;

#endregion

namespace zora.Tests.Utils;

public static class HttpUtils
{
    public static ClaimsPrincipal GenerateClaimsPrincipal(string name, List<string> roles, string id)
    {
        ClaimsIdentity identity = new ClaimsIdentity([
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.NameIdentifier, id),
            new Claim("UserId", id)
        ], "TestAuthentication");

        identity.AddClaims(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return new ClaimsPrincipal(identity);
    }
}
