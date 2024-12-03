#region

using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces;

public interface IPermissionRepository : IZoraService
{
    Task<Permission?> GetByNameAsync(string name);
    Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(long roleId);
}
