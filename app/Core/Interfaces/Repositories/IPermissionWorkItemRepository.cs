#region

using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface IPermissionWorkItemRepository
{
    Task<PermissionWorkItem?> GetByCompositeKeyAsync(long permissionId, long requestResourceId);
}
