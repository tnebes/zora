#region

using System.Linq.Expressions;
using zora.Core;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Requests.Interfaces;
using zora.Core.Enums;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public sealed class QueryService : IQueryService, IZoraService
{
    private readonly ILogger<QueryService> _logger;
    public QueryService(ILogger<QueryService> logger) => this._logger = logger;

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

    public IQueryable<T> GetEntityQueryable<T>(DynamicQueryParamsDto queryParams, IQueryable<T> query) where T : class
    {
        try
        {
            IQueryable<T> queryable = (typeof(T) switch
            {
                { } t when t == typeof(User) => this.GetQueryableUser(
                    (DynamicQueryUserParamsDto)queryParams
                    ,(IQueryable<User>)query) as IQueryable<T>,
                { } t when t == typeof(Role) => this.GetQueryableRole(
                    (DynamicQueryRoleParamsDto)queryParams
                    ,(IQueryable<Role>)query) as IQueryable<T>,
                { } t when t == typeof(Permission) => this.GetQueryablePermission(
                    (DynamicQueryPermissionParamsDto)queryParams
                    ,(IQueryable<Permission>)query) as IQueryable<T>,
                _ => null
            })!;

            if (queryable == null)
            {
                this._logger.LogError("Invalid type {Type}", typeof(T));
                throw new ArgumentOutOfRangeException(nameof(T), "Invalid type");
            }

            return queryable;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting queryable {Type}", typeof(T));
            throw;
        }
    }

    private IQueryable<Role> GetQueryableRole(DynamicQueryRoleParamsDto queryParams, IQueryable<Role> query)
    {
        this.ApplyListFilter(ref query, queryParams.Id, long.Parse,
            (role, ids) => ids.Contains(role.Id));

        this.ApplyListFilter(ref query, queryParams.Name, s => s,
            (role, names) => names.Contains(role.Name));

        this.ApplyListFilter(ref query, queryParams.Permission, long.Parse,
            (role, permissions) => role.RolePermissions.Any(rp => permissions.Contains(rp.Permission.Id)));

        this.ApplyListFilter(ref query, queryParams.User, long.Parse,
            (role, users) => role.UserRoles.Any(ur => users.Contains(ur.User.Id)));

        this.ApplyFilter(ref query, queryParams.CreatedAt,
            (role, date) => role.CreatedAt == date);

        return query;
    }

    private IQueryable<User> GetQueryableUser(DynamicQueryUserParamsDto queryParams, IQueryable<User> query)
    {
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

        this.ApplyFilter(ref query, queryParams.CreatedAt,
            (user, date) => date.Equals(user.CreatedAt));

        return query;
    }

    private IQueryable<Permission> GetQueryablePermission(DynamicQueryPermissionParamsDto queryParams,
        IQueryable<Permission> query)
    {
        this.ApplyListFilter(ref query, queryParams.Id, long.Parse,
            (permission, ids) => ids.Contains(permission.Id));

        this.ApplyListFilter(ref query, queryParams.Name, s => s,
            (permission, names) => names.Contains(permission.Name));

        this.ApplyListFilter(ref query, queryParams.Description, s => s,
            (permission, descriptions) => descriptions.Contains(permission.Description));

        this.ApplyListFilter(ref query, queryParams.PermissionString, s => s,
            (permission, permissionStrings) => permissionStrings.Contains(permission.PermissionString));

        this.ApplyListFilter(ref query, queryParams.RoleIds, long.Parse,
            (permission, roles) => permission.RolePermissions.Any(rp => roles.Contains(rp.Role.Id)));

        this.ApplyListFilter(ref query, queryParams.WorkItemIds, long.Parse,
            (permission, workItems) => permission.PermissionWorkItems.Any(pw => workItems.Contains(pw.WorkItem.Id)));

        return query;
    }

    private void ApplyFilter<TEntity, TValue>(
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
