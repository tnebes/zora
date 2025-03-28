#region

using FluentResults;
using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface IPermissionWorkItemRepository
{
    Task<Result<PermissionWorkItem>> GetByCompositeKeyAsync(long permissionId, long requestResourceId,
        bool includeProperties = false);

    Task<Result<PermissionWorkItem>> CreateAsync(PermissionWorkItem permissionWorkItem);

    Task<bool> DeleteAsync(PermissionWorkItem permissionWorkItem);

    Task<Result<IEnumerable<PermissionWorkItem>>> CreateRangeAsync(IEnumerable<PermissionWorkItem> permissionWorkItems);

    Task<Result<IEnumerable<PermissionWorkItem>>> GetByPermissionIdAsync(long permissionId, bool includeProperties = false);
}
