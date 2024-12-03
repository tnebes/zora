#region

using Microsoft.EntityFrameworkCore;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.Interfaces;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.Repositories;

[ServiceLifetime(ServiceLifetime.Scoped)]
public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(
        ApplicationDbContext dbContext,
        ILogger<UserRepository> logger)
        : base(dbContext, logger)
    {
    }

    public async Task<User> GetUserByUsernameAsync(string username)
    {
        try
        {
            return await this.DbSet
                       .FirstOrDefaultAsync(u => u.Username == username)
                   ?? throw new KeyNotFoundException($"User with username '{username}' not found.");
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error retrieving user by username {Username}", username);
            throw;
        }
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        try
        {
            return await this.DbSet
                       .FirstOrDefaultAsync(u => u.Email == email)
                   ?? throw new KeyNotFoundException($"User with email '{email}' not found.");
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error retrieving user by email {Email}", email);
            throw;
        }
    }

    public async Task<bool> ValidateUserCredentialsAsync(string username, string password)
    {
        try
        {
            User? user = await this.DbSet
                .FirstOrDefaultAsync(u => u.Username == username);

            // Note: You should implement proper password hashing here
            return user?.Password == password;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error validating credentials for username {Username}", username);
            throw;
        }
    }

    public async Task<User> GetUserByIdAsync(long id)
    {
        try
        {
            return await this.DbSet
                       .Include(u => u.Roles)
                       .ThenInclude(r => r.Permissions)
                       .FirstOrDefaultAsync(u => u.Id == id)
                   ?? throw new KeyNotFoundException($"User with ID '{id}' not found.");
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error retrieving user by ID {Id}", id);
            throw;
        }
    }

    public Task<IEnumerable<User>> GetUsersWithRolesAsync() => throw new NotImplementedException();

    public Task<bool> IsUsernameUniqueAsync(string username) => throw new NotImplementedException();

    public Task<bool> IsEmailUniqueAsync(string email) => throw new NotImplementedException();
}
