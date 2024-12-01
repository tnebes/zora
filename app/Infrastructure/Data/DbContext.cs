#region

using Microsoft.Data.SqlClient;
using zora.Core.Attributes;
using zora.Core.Interfaces;
using Constants = zora.Core.Constants;

#endregion

namespace zora.Infrastructure.Data;

[ServiceLifetime(ServiceLifetime.Scoped)]
public class DbContext : IDbContext
{
    private readonly string? _connectionString;
    private readonly ILogger<DbContext> _logger;

    public DbContext(IConfiguration configuration, ILogger<DbContext> logger)
    {
        this._logger = logger;
        this._connectionString = configuration.GetConnectionString(Constants.ConnectionStringKey);
        if (string.IsNullOrEmpty(this._connectionString))
        {
            this._logger.LogError("Database connection string {KeyName} not found in secrets. Use dotnet user-secrets.",
                Constants.ConnectionStringKey);
            throw new InvalidOperationException(
                "Database connection string" + Constants.ConnectionStringKey +
                "not found in environment variables. Use dotnet user-secrets.");
        }
    }

    public async Task<SqlConnection> CreateConnectionAsync()
    {
        try
        {
            SqlConnection connection = new(this._connectionString);
            await connection.OpenAsync();
            return connection;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error creating database connection");
            throw;
        }
    }
}
