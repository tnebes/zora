#region

using zora.Core;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Requests.Interfaces;
using zora.Core.Enums;
using zora.Core.Interfaces.Services;
using zora.Infrastructure.Data;
using System.Linq.Expressions;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public sealed class QueryService : IQueryService, IZoraService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<QueryService> _logger;
    private readonly Dictionary<Type, Func<DynamicQueryParamsDto, IQueryable<object>>> _queryBuilders;

    public QueryService(ILogger<QueryService> logger, ApplicationDbContext dbContext)
    {
        this._logger = logger;
        this._dbContext = dbContext;
        this._queryBuilders = new Dictionary<Type, Func<DynamicQueryParamsDto, IQueryable<object>>>
        {
            { typeof(User), query => this.GetQueryableUser((DynamicQueryUserParamsDto)query) },
            { typeof(Role), query => this.GetQueryableRole((DynamicQueryRoleParamsDto)query) },
            { typeof(Permission), query => this.GetQueryablePermission((DynamicQueryPermissionParamsDto)query) }
        };
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
        if (!this._queryBuilders.TryGetValue(typeof(T), out Func<DynamicQueryParamsDto, IQueryable<object>>? queryBuilder))
        {
            this._logger.LogError("Invalid type {Type}", typeof(T));
            throw new ArgumentOutOfRangeException(nameof(T), "Invalid type");
        }

        return (IQueryable<T>)queryBuilder(queryParams);
    }

    private IQueryable<Role> GetQueryableRole(DynamicQueryRoleParamsDto queryParams)
    {
        IQueryable<Role> query = this._dbContext.Roles.AsQueryable();

        this.ApplyListFilter(ref query, queryParams.Id, long.Parse,
            (role, ids) => ids.Contains(role.Id));

        this.ApplyListFilter(ref query, queryParams.Name, s => s,
            (role, names) => names.Contains(role.Name));

        this.ApplyListFilter(ref query, queryParams.Permission, long.Parse,
            (role, permissions) => role.RolePermissions.Any(rp => permissions.Contains(rp.Permission.Id)));

        this.ApplyListFilter(ref query, queryParams.User, long.Parse,
            (role, users) => role.UserRoles.Any(ur => users.Contains(ur.User.Id)));

        QueryService.ApplyFilter(ref query, queryParams.CreatedAt,
            (role, date) => role.CreatedAt == date);

        return query;
    }

    private IQueryable<User> GetQueryableUser(DynamicQueryUserParamsDto queryParams)
    {
        IQueryable<User> query = this._dbContext.Users.AsQueryable();

        this.ApplyListFilter(ref query, queryParams.Id, long.Parse,
            (user, ids) => ids.Contains(user.Id));

        this.ApplyListFilter(ref query, queryParams.Username, s => s,
            (user, usernames) => usernames.Contains(user.Username));

        this.ApplyListFilter(ref query, queryParams.Email, s => s,
            (user, emails) => emails.Contains(user.Email));

        this.ApplyListFilter(ref query, queryParams.Role, long.Parse,
            (user, roles) => user.UserRoles.Any(ur => roles.Contains(ur.Role.Id)));

        this.ApplyListFilter(ref query, queryParams.Permission, long.Parse,
            (user, permissions) => user.UserRoles.Any(ur =>
                ur.Role.RolePermissions.Any(rp => permissions.Contains(rp.Permission.Id))));

        QueryService.ApplyFilter(ref query, queryParams.CreatedAt,
            (user, date) => date.Equals(user.CreatedAt));

        return query;
    }

    private IQueryable<Permission> GetQueryablePermission(DynamicQueryPermissionParamsDto queryParams)
    {
        IQueryable<Permission> query = this._dbContext.Permissions.AsQueryable();

        QueryService.ApplyFilter(ref query, queryParams.Name,
            (permission, name) => permission.Name.Contains(name));

        QueryService.ApplyFilter(ref query, queryParams.Description,
            (permission, description) => permission.Description.Contains(description));

        QueryService.ApplyFilter(ref query, queryParams.PermissionString,
            (permission, permissionString) => permission.PermissionString.Contains(permissionString));

        this.ApplyListFilter(ref query, queryParams.RoleIds, long.Parse,
            (permission, roles) => permission.RolePermissions.Any(rp => roles.Contains(rp.Role.Id)));

        this.ApplyListFilter(ref query, queryParams.WorkItemIds, long.Parse,
            (permission, workItems) => permission.PermissionWorkItems.Any(pw => workItems.Contains(pw.WorkItem.Id)));

        return query;
    }

    private static void ApplyFilter<TEntity, TValue>(
        ref IQueryable<TEntity> query,
        TValue value,
        Expression<Func<TEntity, TValue, bool>> predicate)
    {
        if (value != null)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(TEntity));
            InvocationExpression body = Expression.Invoke(predicate, parameter, Expression.Constant(value));
            query = query.Where(Expression.Lambda<Func<TEntity, bool>>(body, parameter));
        }
    }

    private void ApplyListFilter<TEntity, TValue>(
        ref IQueryable<TEntity> query,
        string input,
        Func<string, TValue> parser,
        Expression<Func<TEntity, List<TValue>, bool>> predicate)
    {
        if (!string.IsNullOrWhiteSpace(input))
        {
            List<TValue> values = this.ParseStringToList(input, parser);
            ParameterExpression parameter = Expression.Parameter(typeof(TEntity));
            InvocationExpression body = Expression.Invoke(predicate, parameter, Expression.Constant(values));
            query = query.Where(Expression.Lambda<Func<TEntity, bool>>(body, parameter));
        }
    }

    private List<T> ParseStringToList<T>(string input, Func<string, T> parser)
    {
        return input.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(parser)
            .ToList();
    }
}
