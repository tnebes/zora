#region

using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces;

public interface IRoleRepository : IZoraService
{
    Task<Role?> GetByNameAsync(string name);
    Task<IEnumerable<Role>> GetRolesWithPermissionsAsync();
}
