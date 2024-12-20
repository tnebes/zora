#region

using Microsoft.EntityFrameworkCore;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.Interfaces.Repositories;
using zora.Core.Interfaces.Services;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.Repositories;

public sealed class UserRepository : BaseRepository<User>, IUserRepository, IZoraService
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    public UserRepository(
        ApplicationDbContext dbContext,
        ILogger<UserRepository> logger) : base(dbContext, logger)
    {
    }

    public new async Task<User?> GetByIdAsync(long id)
    {
        await UserRepository.Semaphore.WaitAsync();
        try
        {
            return await base.GetByIdAsync(id);
        }
        finally
        {
            UserRepository.Semaphore.Release();
        }
    }

    public async Task<User?> GetByUsernameAsync(string username) =>
        await this.FindByCondition(user => user.Username == username).FirstOrDefaultAsync();

    public async Task<(IQueryable<User>, int totalCount)> GetUsersAsync(QueryParamsDto queryParams) =>
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

    public void Delete(User user) => this.DbSet.Remove(user);
    public Task SaveChangesAsync() => this.DbContext.SaveChangesAsync();

    public void SoftDelete(User user) => this.DbSet.Update(user);
    public async Task<User> Add(User user) => await this.AddAsync(user);
}
