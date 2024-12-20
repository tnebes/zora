#region

using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface IRoleRepository
{
    IQueryable<Role> GetRoles(IEnumerable<long> roleIds);
}
