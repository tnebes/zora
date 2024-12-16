#region

using zora.Core;
using zora.Core.Domain;
using zora.Core.Interfaces;

#endregion

namespace zora.Infrastructure.Services;

public class UserRoleService : IUserRoleService, IZoraService
{
    private readonly ILogger<UserRoleService> _logger;
    private readonly IUserRoleRepository _userRoleRepository;

    public UserRoleService(IUserRoleRepository userRoleRepository, ILogger<UserRoleService> logger)
    {
        this._userRoleRepository = userRoleRepository;
        this._logger = logger;
    }

    public Task<IEnumerable<UserRole>> GetUserRolesByUserIdAsync(long userId) =>
        this._userRoleRepository.GetByUserIdAsync(userId);

    public Task<bool> IsRoleAsync(IEnumerable<UserRole> userRoles, string roleName) =>
        Task.FromResult(userRoles.Any(role => role.Role.Name == roleName));

    public Task<bool> IsRoleAsync(long userId, string roleName)
    {
        IEnumerable<UserRole> userRoles = this.GetUserRolesByUserIdAsync(userId).Result;
        return this.IsRoleAsync(userRoles, roleName);
    }

    public Task<bool> IsAdminAsync(IEnumerable<UserRole> userRoles) => this.IsRoleAsync(userRoles, Constants.ADMIN);

    public Task<bool> IsAdminAsync(long userId) => this.IsRoleAsync(userId, Constants.ADMIN);
}
