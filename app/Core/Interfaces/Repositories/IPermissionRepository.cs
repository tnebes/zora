#region

using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface IPermissionRepository
{
    Task<Permission?> GetByIdAsync(long rolePermissionPermissionId);
}
