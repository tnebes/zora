#region

using System.Security.Claims;
using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces.Services;

public interface IRoleService
{
    Task<bool> IsRole(ClaimsPrincipal httpContextUser, string role);
    Task<bool> IsAdmin(ClaimsPrincipal httpContextUser);
    Task<bool> AssignRoles(User user, IEnumerable<long> roleIds);
}
