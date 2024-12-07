using zora.Core.Domain;

namespace zora.Core.Interfaces;

public interface IPermissionRepository
{
    Task<Permission?> GetByIdAsync(long rolePermissionPermissionId);
}
