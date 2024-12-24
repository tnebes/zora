#region

using FluentResults;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface IUserRepository
{
    Task<Result<User>> GetByIdAsync(long id);
    Task<Result<User>> GetByUsernameAsync(string username);
    Task<(IEnumerable<User>, int totalCount)> GetUsersAsync(QueryParamsDto queryParams);
    Task<Result<User>> GetByEmailAsync(string email);
    Task<bool> IsUsernameUniqueAsync(string username);
    Task<bool> IsEmailUniqueAsync(string email);
    void Delete(User user);
    Task SaveChangesAsync();
    void SoftDelete(User user);
    Task<User> Add(User user);
    Task<Result<User>> Update(User originalUser);
    Task<Result<(IEnumerable<User> users, int totalCount)>> SearchUsers(IQueryable<User> query);
    Task<Result<(IEnumerable<User>, int totalCount)>> FindUsersAsync(QueryParamsDto findParams);
}
