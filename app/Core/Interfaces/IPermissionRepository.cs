#region

using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces;

public interface IPermissionRepository
{
    Task<Permission?> GetByIdAsync(long rolePermissionPermissionId);
}
