#region

using System.Security.Claims;

#endregion

namespace zora.Core.Interfaces;

public interface IRoleService
{
    Task<bool> IsRole(ClaimsPrincipal httpContextUser, string role);
    Task<bool> IsAdmin(ClaimsPrincipal httpContextUser);
}
