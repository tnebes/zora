#region

using System.Security.Claims;

#endregion

namespace zora.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static long GetUserId(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        Claim? userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim is null || string.IsNullOrWhiteSpace(userIdClaim.Value))
        {
            throw new UnauthorizedAccessException("User ID claim is missing");
        }

        if (!long.TryParse(userIdClaim.Value, out long userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID format");
        }

        return userId;
    }
}
