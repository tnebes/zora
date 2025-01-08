#region

using FluentResults;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface IUserRepository
{
    Task<Result<User>> GetByIdAsync(long id, bool includeProperties = false);
    Task<Result<User>> GetByUsernameAsync(string username, bool includeProperties = false);
    Task<(IEnumerable<User>, int totalCount)> GetUsersAsync(QueryParamsDto queryParams, bool includeProperties = false);
    Task<Result<User>> GetByEmailAsync(string email, bool includeProperties = false);
    Task<bool> IsUsernameUniqueAsync(string username);
    Task<bool> IsEmailUniqueAsync(string email);
    Task SoftDelete(User user);
    Task<User> Add(User user);
    Task<Result<User>> Update(User originalUser);

    Task<Result<(IEnumerable<User> users, int totalCount)>> SearchUsers(IQueryable<User> query,
        bool includeProperties = false);

    Task<Result<(IEnumerable<User>, int totalCount)>> FindUsersAsync(QueryParamsDto findParams,
        bool includeProperties = false);
}
