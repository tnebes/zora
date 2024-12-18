#region

using Microsoft.EntityFrameworkCore;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.Interfaces;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository, IZoraService
{
    public UserRepository(
        ApplicationDbContext dbContext,
        ILogger<UserRepository> logger) : base(dbContext, logger)
    {
    }

    public new async Task<User?> GetByIdAsync(long id) => await this.DbSet.FirstOrDefaultAsync(user => user.Id == id);

    public async Task<User?> GetByUsernameAsync(string username) =>
        await this.FindByCondition(user => user.Username == username).FirstOrDefaultAsync();

    public async Task<(IEnumerable<User>, int totalCount)> GetUsersAsync(QueryParamsDto queryParams) =>
        await this.GetPagedAsync(queryParams.Page, queryParams.PageSize);

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await this.FindByCondition(user => user.Email == email)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IsUsernameUniqueAsync(string username)
    {
        return !await this.FindByCondition(user => user.Username == username)
            .AnyAsync();
    }

    public async Task<bool> IsEmailUniqueAsync(string email)
    {
        return !await this.FindByCondition(user => user.Email == email)
            .AnyAsync();
    }
}
