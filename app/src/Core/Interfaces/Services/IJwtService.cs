#region

using System.Security.Claims;
using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces.Services;

public interface IJwtService
{
    string GenerateToken(User user);
    int GetTokenExpiration();
    long GetCurrentUserId(ClaimsPrincipal user);
}
