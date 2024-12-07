using zora.Core.Domain;

namespace zora.Core.Interfaces;

public interface IPermissionWorkItemRepository
{
    Task<PermissionWorkItem?> GetByCompositeKeyAsync(long permissionId, long requestResourceId);
}
