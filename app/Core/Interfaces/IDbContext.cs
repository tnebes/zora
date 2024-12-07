#region

using Microsoft.Data.SqlClient;

#endregion

namespace zora.Core.Interfaces;

public interface IDbContext
{
    Task<SqlConnection> CreateConnectionAsync();
}
