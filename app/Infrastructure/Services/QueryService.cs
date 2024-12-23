#region

using zora.Core;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Requests.Interfaces;
using zora.Core.Enums;
using zora.Core.Interfaces.Services;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public class QueryService : IQueryService, IZoraService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<QueryService> _logger;

    public QueryService(ILogger<QueryService> logger, ApplicationDbContext dbContext)
    {
        this._logger = logger;
        this._dbContext = dbContext;
    }

    public void NormaliseQueryParams(IQueryParamsDto queryParams)
    {
        queryParams.Page = Math.Max(1, queryParams.Page);
        queryParams.PageSize = Math.Clamp(queryParams.PageSize, Constants.DEFAULT_PAGE_SIZE,
            Constants.MAX_RESULTS_PER_PAGE);
    }

    public void ValidateQueryParams(DynamicQueryParamsDto queryParams, ResourceType type)
    {
        if (type == ResourceType.Route)
        {
            throw new ArgumentOutOfRangeException(nameof(type), "Invalid resource type");
        }
    }

    public IQueryable<T> GetEntityQueryable<T>(DynamicQueryParamsDto queryParams)
    {
        switch (typeof(T))
        {
            case { } user when user == typeof(User):
                return this.GetQueryableUser((DynamicQueryUserParamsDto)queryParams) as IQueryable<T>;
            case { } role when role == typeof(Role):
                // return this.GetQueryableRole(queryParams) as IQueryable<T>;
                throw new NotImplementedException();
            case { } permission when permission == typeof(Permission):
                // return this.GetQueryablePermission(queryParams) as IQueryable<T>;
                throw new NotImplementedException();
            default:
                this._logger.LogError("Invalid type {T}", typeof(T));
                throw new ArgumentOutOfRangeException(nameof(T), "Invalid type");
        }
    }

    private IQueryable<User> GetQueryableUser(DynamicQueryUserParamsDto queryParams)
    {
        IQueryable<User> query = this._dbContext.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryParams.Id))
        {
            List<long> ids = this.ParseStringToList(queryParams.Id, long.Parse);
            query = query.Where(user => ids.Contains(user.Id));
        }

        if (!string.IsNullOrWhiteSpace(queryParams.Username))
        {
            List<string> usernames = this.ParseStringToList(queryParams.Username, s => s);
            query = query.Where(user => usernames.Contains(user.Username));
        }

        if (!string.IsNullOrWhiteSpace(queryParams.Email))
        {
            List<string> emails = this.ParseStringToList(queryParams.Email, s => s);
            query = query.Where(user => emails.Contains(user.Email));
        }

        if (!string.IsNullOrWhiteSpace(queryParams.Role))
        {
            List<long> roles = this.ParseStringToList(queryParams.Role, long.Parse);
            query = query.Where(user => user.UserRoles.Any(userRole => roles.Contains(userRole.Role.Id)));
        }

        if (!string.IsNullOrWhiteSpace(queryParams.Permission))
        {
            List<long> permissions = this.ParseStringToList(queryParams.Permission, long.Parse);
            query = query.Where(user => user.UserRoles.Any(userRole =>
                userRole.Role.RolePermissions.Any(rolePermission =>
                    permissions.Contains(rolePermission.Permission.Id))));
        }

        if (queryParams.CreatedAt != null)
        {
            query = query.Where(user => queryParams.CreatedAt.Equals(user.CreatedAt));
        }

        return query;
    }

    private List<T> ParseStringToList<T>(string input, Func<string, T> parser)
    {
        return input.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(parser)
            .ToList();
    }
}
