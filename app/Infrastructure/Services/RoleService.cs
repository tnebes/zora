#region

using System.Security.Claims;
using zora.Core.Interfaces;
using Constants = zora.Core.Constants;

#endregion

namespace zora.Infrastructure.Services;

public class RoleService : IRoleService, IZoraService
{
    public Task<bool> IsRole(ClaimsPrincipal httpContextUser, string role)
    {
        if (httpContextUser == null)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(httpContextUser.IsInRole(role));
    }

    public Task<bool> IsAdmin(ClaimsPrincipal httpContextUser) => this.IsRole(httpContextUser, Constants.ADMIN);
}
