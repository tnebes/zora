#region

using zora.Core;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.Interfaces.Repositories;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public class UserRoleService : IUserRoleService, IZoraService
{
    private readonly ILogger<UserRoleService> _logger;
    private readonly IUserRoleRepository _userRoleRepository;

    public UserRoleService(IUserRoleRepository userRoleRepository, ILogger<UserRoleService> logger)
    {
        this._userRoleRepository = userRoleRepository;
        this._logger = logger;
    }

    public async Task<IEnumerable<UserRole>> GetUserRolesByUserIdAsync(long userId) =>
        await this._userRoleRepository.GetByUserIdAsync(userId);


    public Task<bool> IsRoleAsync(IEnumerable<UserRole> userRoles, string roleName) =>
        Task.FromResult(userRoles.Any(role => role.Role.Name == roleName));

    public async Task<bool> IsRoleAsync(long userId, string roleName)
    {
        IEnumerable<UserRole> userRoles = await this.GetUserRolesByUserIdAsync(userId);
        return await this.IsRoleAsync(userRoles, roleName);
    }

    public Task<bool> IsAdminAsync(IEnumerable<UserRole> userRoles) => this.IsRoleAsync(userRoles, Constants.ADMIN);

    public Task<bool> IsAdminAsync(long userId) => this.IsRoleAsync(userId, Constants.ADMIN);
}
