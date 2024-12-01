using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using zora.Core.Attributes;
using zora.Core.Interfaces;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace zora.Infrastructure.Data;

[ServiceLifetime(ServiceLifetime.Scoped)]
public class DbContext : IDbContext
{
    private readonly string? _connectionString;
    private readonly ILogger<DbContext> _logger;

    public DbContext(IConfiguration configuration, ILogger<DbContext> logger)
    {
        this._logger = logger;
        this._connectionString = Environment.GetEnvironmentVariable("ZORA_DB_CONNECTION");
        if (string.IsNullOrEmpty(this._connectionString))
        {
            this._logger.LogError("Database connection string (ZORA_DB_CONNECTION) not found in environment variables");
            throw new InvalidOperationException(
                "Database connection string (ZORA_DB_CONNECTION) not found in environment variables");
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
