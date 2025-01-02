#region

using FluentResults;
using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface IPermissionWorkItemRepository
{
    Task<Result<PermissionWorkItem>> GetByCompositeKeyAsync(long permissionId, long requestResourceId);
}
