#region

using AutoMapper;
using FluentResults;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;
using zora.Core.Enums;
using zora.Core.Interfaces.Repositories;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public sealed class PermissionService : IPermissionService, IZoraService
{
    private readonly ILogger<PermissionService> _logger;
    private readonly IMapper _mapper;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IPermissionWorkItemRepository _permissionWorkItemRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IUserRoleService _userRoleService;

    public PermissionService(
        ILogger<PermissionService> logger,
        IRolePermissionRepository rolePermissionRepository,
        IPermissionRepository permissionRepository,
        IPermissionWorkItemRepository permissionWorkItemRepository,
        IUserRoleService userRoleService,
        IMapper mapper)
    {
        this._logger = logger;
        this._rolePermissionRepository = rolePermissionRepository;
        this._permissionRepository = permissionRepository;
        this._permissionWorkItemRepository = permissionWorkItemRepository;
        this._userRoleService = userRoleService;
        this._mapper = mapper;
    }

    public async Task<Result<(IEnumerable<Permission>, int total)>> GetAsync(QueryParamsDto queryParams)
    {
        try
        {
            return await this._permissionRepository.GetPagedAsync(queryParams);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting permissions");
            return Result.Fail<(IEnumerable<Permission>, int total)>("Failed to retrieve permissions");
        }
    }

    public async Task<Result<PermissionResponseDto>> GetDtoAsync(QueryParamsDto queryParams)
    {
        try
        {
            Result<(IEnumerable<Permission> permissions, int total)> result =
                await this._permissionRepository.GetPagedAsync(queryParams);

            if (result.IsFailed)
            {
                return Result.Fail<PermissionResponseDto>("Failed to retrieve permissions");
            }

            (IEnumerable<Permission> permissions, int total) = result.Value;

            PermissionResponseDto response = new PermissionResponseDto
            {
                Items = this._mapper.Map<IEnumerable<PermissionDto>>(permissions),
                Total = total,
                Page = queryParams.Page,
                PageSize = queryParams.PageSize
            };

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting permissions");
            return Result.Fail<PermissionResponseDto>("Failed to retrieve permissions");
        }
    }

    public async Task<Result<Permission>> GetByIdAsync(long id)
    {
        try
        {
            return await this._permissionRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting permission by id {Id}", id);
            return Result.Fail<Permission>("Failed to retrieve permission");
        }
    }

    public async Task<Result<Permission>> CreateAsync(CreatePermissionDto permissionDto)
    {
        try
        {
            Permission permission = new Permission
            {
                Name = permissionDto.Name,
                Description = permissionDto.Description,
                PermissionString = permissionDto.PermissionString,
                CreatedAt = DateTime.UtcNow
            };

            await this._permissionRepository.CreateAsync(permission);
            return Result.Ok(permission);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error creating permission");
            return Result.Fail<Permission>("Failed to create permission");
        }
    }

    public async Task<Result<Permission>> UpdateAsync(long id, UpdatePermissionDto updateDto)
    {
        try
        {
            Result<Permission> permissionResult = await this._permissionRepository.GetByIdAsync(id);
            if (permissionResult.IsFailed)
            {
                return Result.Fail<Permission>($"Permission with ID {id} not found");
            }

            Permission permission = permissionResult.Value;
            permission.Name = updateDto.Name;
            permission.Description = updateDto.Description;
            permission.PermissionString = updateDto.PermissionString;

            Result<Permission> result = await this._permissionRepository.UpdateAsync(permission);
            return result;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error updating permission");
            return Result.Fail<Permission>("Failed to update permission");
        }
    }

    public async Task<bool> DeleteAsync(long id)
    {
        try
        {
            Result<Permission> permissionResult = await this._permissionRepository.GetByIdAsync(id);
            if (permissionResult.IsFailed)
            {
                return false;
            }

            Permission permission = permissionResult.Value;
            permission.Deleted = true;
            return await this._permissionRepository.DeleteAsync(permission);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error deleting permission with id {Id}", id);
            return false;
        }
    }

    public async Task<Result<PermissionResponseDto>> FindAsync(QueryParamsDto findParams)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(findParams.SearchTerm) || findParams.SearchTerm.Length < 3)
            {
                return Result.Fail<PermissionResponseDto>("Search term must be at least 3 characters long");
            }

            Result<(IEnumerable<Permission>, int totalCount)> result =
                await this._permissionRepository.FindPermissionsAsync(findParams);

            if (result.IsFailed)
            {
                return Result.Fail<PermissionResponseDto>("Error finding permissions");
            }

            (IEnumerable<Permission> permissions, int total) = result.Value;

            PermissionResponseDto response = new PermissionResponseDto
            {
                Items = this._mapper.Map<IEnumerable<PermissionDto>>(permissions),
                Total = total,
                Page = findParams.Page,
                PageSize = findParams.PageSize
            };

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error finding permissions with term {SearchTerm}", findParams.SearchTerm);
            return Result.Fail<PermissionResponseDto>("Error finding permissions");
        }
    }

    public async Task<Result<PermissionResponseDto>> SearchAsync(DynamicQueryPermissionParamsDto searchParams)
    {
        try
        {
            Result<(IEnumerable<Permission>, int TotalCount)> result =
                await this._permissionRepository.SearchAsync(searchParams, true);
            if (result.IsFailed)
            {
                return Result.Fail<PermissionResponseDto>("Error searching permissions");
            }

            (IEnumerable<Permission> permissions, int total) = result.Value;
            IEnumerable<PermissionDto> permissionDtos = this._mapper.Map<IEnumerable<PermissionDto>>(permissions);

            return Result.Ok(new PermissionResponseDto
                { Items = permissionDtos, Total = total, Page = searchParams.Page, PageSize = searchParams.PageSize });
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error searching permissions");
            return Result.Fail<PermissionResponseDto>("Error searching permissions");
        }
    }

    public async Task<bool> HasDirectPermissionAsync(PermissionRequestDto request)
    {
        try
        {
            List<UserRole> userRoles = (await this._userRoleService.GetUserRolesByUserIdAsync(request.UserId)).ToList();

            if (await this._userRoleService.IsAdminAsync(userRoles))
            {
                this._logger.LogInformation("User {UserId} is an administrator and has all permissions",
                    request.UserId);
                return true;
            }

            foreach (UserRole userRole in userRoles)
            {
                Result<IEnumerable<RolePermission>> result =
                    await this._rolePermissionRepository.GetByRoleIdAsync(userRole.RoleId);

                if (result.IsFailed)
                {
                    this._logger.LogWarning("Warning: Could not get role permissions for role {RoleId}",
                        userRole.RoleId);
                    continue;
                }

                IEnumerable<RolePermission> rolePermissions = result.Value;

                foreach (RolePermission rolePermission in rolePermissions)
                {
                    Result<Permission> permissionResult =
                        await this._permissionRepository.GetByIdAsync(rolePermission.PermissionId);

                    if (permissionResult.IsFailed)
                    {
                        continue;
                    }

                    Permission permission = permissionResult.Value;

                    Result<PermissionWorkItem> permissionWorkItemResult =
                        await this._permissionWorkItemRepository.GetByCompositeKeyAsync(permission.Id,
                            request.ResourceId);

                    if (permissionWorkItemResult.IsFailed)
                    {
                        continue;
                    }

                    PermissionWorkItem resourcePermission = permissionWorkItemResult.Value;

                    if (resourcePermission != null &&
                        PermissionService.DoesPermissionGrantAccess(permission.PermissionString,
                            request.RequestedPermission))
                    {
                        this._logger.LogInformation(
                            "Direct permission found for user {UserId} on resource {ResourceId}",
                            request.UserId, request.ResourceId);
                        return true;
                    }
                }
            }

            this._logger.LogInformation(
                "No direct permission found for user {UserId} on resource {ResourceId}",
                request.UserId, request.ResourceId);
            return false;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex,
                "Error checking direct permissions for user {UserId} on resource {ResourceId}",
                request.UserId, request.ResourceId);
            return false;
        }
    }

    public async Task<Result<IEnumerable<Permission>>> GetAllAsync()
    {
        try
        {
            Result<IEnumerable<Permission>> result = await this._permissionRepository.GetAllAsync();
            return result.IsSuccess
                ? Result.Ok(result.Value)
                : Result.Fail<IEnumerable<Permission>>("Failed to retrieve permissions");
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting all permissions");
            return Result.Fail<IEnumerable<Permission>>("Failed to retrieve permissions");
        }
    }

    public async Task<Result<PermissionResponseDto>> GetPermissionsByIdsAsync(List<long> ids)
    {
        try
        {
            Result<(IEnumerable<Permission>, int totalCount)> result =
                await this._permissionRepository.GetByIdsAsync(ids);

            if (result.IsFailed)
            {
                return Result.Fail<PermissionResponseDto>("Failed to retrieve permissions by ids");
            }

            (IEnumerable<Permission> permissions, int total) = result.Value;

            PermissionResponseDto response = new PermissionResponseDto
            {
                Items = this._mapper.Map<IEnumerable<PermissionDto>>(permissions),
                Total = total,
                Page = 1,
                PageSize = total
            };

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting permissions by ids");
            return Result.Fail<PermissionResponseDto>("Failed to retrieve permissions by ids");
        }
    }

    private static bool DoesPermissionGrantAccess(string permissionString, PermissionFlag requestedPermission)
    {
        int permissions = Convert.ToInt32(permissionString, 2);
        return (permissions & (int)requestedPermission) == (int)requestedPermission;
    }
}
