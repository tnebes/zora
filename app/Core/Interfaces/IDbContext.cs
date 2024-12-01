using Microsoft.Data.SqlClient;

namespace zora.Core.Interfaces;

public interface IDbContext : IZoraService
{
    Task<SqlConnection> CreateConnectionAsync();
}
