#region

using System.Security.Claims;
using AutoMapper;
using FluentResults;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;
using zora.Core.Enums;
using zora.Core.Interfaces.Repositories;
using zora.Core.Interfaces.Services;
using zora.Infrastructure.Helpers;
using Constants = zora.Core.Constants;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public sealed class RoleService : IRoleService, IZoraService
{
    private readonly ILogger<RoleService> _logger;
    private readonly IMapper _mapper;
    private readonly IQueryService _queryService;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;

    public RoleService(
        IRoleRepository roleRepository,
        IUserRoleRepository userRoleRepository,
        IRolePermissionRepository rolePermissionRepository,
        IMapper mapper,
        ILogger<RoleService> logger,
        IQueryService queryService)
    {
        this._roleRepository = roleRepository;
        this._userRoleRepository = userRoleRepository;
        this._rolePermissionRepository = rolePermissionRepository;
        this._mapper = mapper;
        this._logger = logger;
        this._queryService = queryService;
    }

    public bool IsRole(ClaimsPrincipal httpContextUser, string role)
    {
        if (httpContextUser == null)
        {
            return false;
        }

        return httpContextUser.IsInRole(role);
    }

    public bool IsAdmin(ClaimsPrincipal httpContextUser) => this.IsRole(httpContextUser, Constants.ADMIN);

    public async Task<bool> AssignRoles(User user, IEnumerable<long> roleIds)
    {
        try
        {
            List<long> roles = roleIds.ToList();
            IEnumerable<Role> existingRoles = await this._roleRepository.GetRolesByIdsAsync(roles);

            if (existingRoles.Count() != roles.Count)
            {
                this._logger.LogError("Not all roles exist");
                return false;
            }

            return await this._userRoleRepository.AssignRoles(user, roles);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error assigning roles to user");
            return false;
        }
    }

    public FullRoleDto MapToFullDto(Role role) => this._mapper.Map<FullRoleDto>(role);

    public async Task<Result<(IEnumerable<Role>, int total)>> GetAsync(QueryParamsDto queryParams)
    {
        try
        {
            (IEnumerable<Role> roles, int total) = await this._roleRepository.GetPagedAsync(queryParams);
            return Result.Ok((roles, total));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting roles");
            return Result.Fail(new Error("Failed to get roles").CausedBy(ex));
        }
    }

    public async Task<Result<RoleResponseDto>> GetDtoAsync(QueryParamsDto queryParams)
    {
        Result<(IEnumerable<Role>, int total)> result = await this.GetAsync(queryParams);
        if (result.IsFailed)
        {
            this._logger.LogError("Error getting roles: {Error}", result.Errors);
            return Result.Fail<RoleResponseDto>(result.Errors);
        }

        (IEnumerable<Role> roles, int total) = result.Value;
        return Result.Ok(roles.ToRoleResponseDto(total, queryParams.Page, queryParams.PageSize, this._mapper));
    }

    public async Task<Result<Role>> GetByIdAsync(long id)
    {
        try
        {
            return await this._roleRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting role by ID {RoleId}", id);
            return Result.Fail<Role>(new Error("Failed to get role by ID").CausedBy(ex));
        }
    }

    public async Task<Result<Role>> CreateAsync(CreateRoleDto createDto)
    {
        try
        {
            Role role = new Role
            {
                Name = createDto.Name,
                CreatedAt = DateTime.UtcNow
            };

            await this._roleRepository.CreateAsync(role);

            List<RolePermission> rolePermissions = createDto.PermissionIds
                .Select(pid => new RolePermission { RoleId = role.Id, PermissionId = pid })
                .ToList();

            await this._rolePermissionRepository.CreateRangeAsync(rolePermissions);
            return await this._roleRepository.GetByIdAsync(role.Id);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error creating role");
            return Result.Fail<Role>(new Error("Failed to create role").CausedBy(ex));
        }
    }

    public async Task<Result<Role>> UpdateAsync(long id, UpdateRoleDto updateDto)
    {
        try
        {
            Result<Role> roleResult = await this._roleRepository.GetByIdAsync(id);
            if (roleResult.IsFailed)
            {
                return Result.Fail<Role>(new Error($"Role with ID {id} not found"));
            }

            Role role = roleResult.Value;
            role.Name = updateDto.Name;

            await this._roleRepository.UpdateAsync(role);
            await this._rolePermissionRepository.DeleteByRoleId(role.Id);

            List<RolePermission> rolePermissions = updateDto.PermissionIds
                .Select(pid => new RolePermission { RoleId = role.Id, PermissionId = pid })
                .ToList();

            await this._rolePermissionRepository.CreateRangeAsync(rolePermissions);
            return await this._roleRepository.GetByIdAsync(role.Id, true);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error updating role");
            return Result.Fail(new Error("Failed to update role").CausedBy(ex));
        }
    }

    public async Task<bool> DeleteAsync(long id)
    {
        try
        {
            Result<Role> roleResult = await this._roleRepository.GetByIdAsync(id);
            if (roleResult.IsFailed)
            {
                return false;
            }

            Role role = roleResult.Value;
            role.Deleted = true;

            return await this._roleRepository.DeleteAsync(role);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error deleting role");
            return false;
        }
    }

    public async Task<Result<RoleResponseDto>> FindAsync(QueryParamsDto findParams)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(findParams.SearchTerm) || findParams.SearchTerm.Length < 3)
            {
                return Result.Fail<RoleResponseDto>("Search term must be at least 3 characters long");
            }

            Result<(IEnumerable<Role>, int totalCount)> result = await this._roleRepository.FindRolesAsync(findParams);

            if (result.IsFailed)
            {
                return Result.Fail<RoleResponseDto>("Error finding roles");
            }

            (IEnumerable<Role> roles, int totalCount) = result.Value;
            return Result.Ok(roles.ToRoleResponseDto(totalCount, findParams.Page, findParams.PageSize, this._mapper));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error finding roles with term {SearchTerm}", findParams.SearchTerm);
            return Result.Fail<RoleResponseDto>("Error finding roles");
        }
    }

    public async Task<Result<RoleResponseDto>> SearchAsync(DynamicQueryRoleParamsDto searchParams)
    {
        try
        {
            this._queryService.ValidateQueryParams(searchParams, ResourceType.Role);
            Result<(IEnumerable<Role> roles, int TotalCount)> searchRoles =
                await this._roleRepository.SearchAsync(searchParams);

            return Result.Ok(searchRoles.Value.roles.ToRoleResponseDto(searchRoles.Value.TotalCount, searchParams.Page,
                searchParams.PageSize, this._mapper));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error searching roles");
            return Result.Fail<RoleResponseDto>(new Error("Error searching roles").CausedBy(ex));
        }
    }
}
