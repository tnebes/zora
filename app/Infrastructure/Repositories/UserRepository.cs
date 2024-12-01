#region

using Dapper;
using Microsoft.Data.SqlClient;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.Interfaces;

#endregion

namespace zora.Infrastructure.Repositories;

[ServiceLifetime(ServiceLifetime.Scoped)]
public class UserRepository : IUserRepository
{
    private readonly IDbContext _dbContext;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IDbContext dbContext, ILogger<UserRepository> logger)
    {
        this._dbContext = dbContext;
        this._logger = logger;
    }

    public async Task<User> GetUserByUsernameAsync(string username)
    {
        try
        {
            const string query = """
                                                 SELECT id AS Id,
                                                        username AS Username,
                                                        password AS Password,
                                                        email AS Email,
                                                        created_at AS CreatedAt
                                                 FROM zora_users
                                                 WHERE username = @Username
                                 """;

            await using SqlConnection connection = await this._dbContext.CreateConnectionAsync();
            return await connection.QueryFirstOrDefaultAsync<User>(query, new { Username = username })
                   ?? throw new KeyNotFoundException($"User with username '{username}' not found.");
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error retrieving user by username {Username}", username);
            throw;
        }
    }

    public Task<User> GetUserByEmailAsync(string email) => throw new NotImplementedException();

    public Task<bool> ValidateUserCredentialsAsync(string username, string password) =>
        throw new NotImplementedException();

    public Task<User> GetUserByIdAsync(long id) => throw new NotImplementedException();
}
