#region

using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.Interfaces.Repositories;
using zora.Core.Interfaces.Services;
using Constants = zora.Core.Constants;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public class RoleService : IRoleService, IZoraService
{
    private readonly ILogger<RoleService> _logger;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;

    public RoleService(IRoleRepository roleRepository, IUserRoleRepository userRoleRepository,
        ILogger<RoleService> logger)
    {
        this._roleRepository = roleRepository;
        this._userRoleRepository = userRoleRepository;
        this._logger = logger;
    }

    public Task<bool> IsRole(ClaimsPrincipal httpContextUser, string role)
    {
        if (httpContextUser == null)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(httpContextUser.IsInRole(role));
    }

    public Task<bool> IsAdmin(ClaimsPrincipal httpContextUser) => this.IsRole(httpContextUser, Constants.ADMIN);

    public async Task<bool> AssignRoles(User user, IEnumerable<long> roleIds)
    {
        List<long> roles = roleIds.ToList();
        if (!await this.IsValid(roles))
        {
            return false;
        }

        return await this._userRoleRepository.AssignRoles(user, roles);
    }

    private async Task<bool> IsValid(IEnumerable<long> roleIds)
    {
        try
        {
            List<Role> roles = await this._roleRepository.GetRoles(roleIds).ToListAsync();
            return roles.Count != 0;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error while checking if roles are valid");
            return false;
        }
    }
}
