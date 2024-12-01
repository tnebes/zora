#region

using Microsoft.Data.SqlClient;

#endregion

namespace zora.Core.Interfaces;

public interface IDbContext : IZoraService
{
    Task<SqlConnection> CreateConnectionAsync();
}
