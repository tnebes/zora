#region

using AutoMapper;
using FluentResults;
using zora.Core;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs.Permissions;
using zora.Core.DTOs.Requests;
using zora.Core.Enums;
using zora.Core.Interfaces.Repositories;
using zora.Core.Interfaces.Services;
using zora.Core.Utilities;

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
                await this._permissionRepository.GetPagedAsync(queryParams, true);

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

    public async Task<Result<Permission>> GetByIdAsync(long id, bool includeProperties = false)
    {
        try
        {
            return await this._permissionRepository.GetByIdAsync(id, includeProperties);
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
            Result<Permission> validationResult = this.validateCreatePermission(permissionDto);

            if (validationResult.IsFailed)
            {
                this._logger.LogError("Error creating permission: {Error}", validationResult.Errors);
                return validationResult;
            }

            Permission permission = new Permission
            {
                Name = permissionDto.Name,
                Description = permissionDto.Description,
                PermissionString = permissionDto.PermissionString,
                CreatedAt = DateTime.UtcNow
            };

            await this._permissionRepository.CreateAsync(permission);

            if (permissionDto.WorkItemIds?.Any() == true)
            {
                foreach (long workItemId in permissionDto.WorkItemIds)
                {
                    PermissionWorkItem permissionWorkItem = new PermissionWorkItem
                    {
                        PermissionId = permission.Id,
                        WorkItemId = workItemId
                    };
                    await this._permissionWorkItemRepository.CreateAsync(permissionWorkItem);
                }
            }

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
            Result<Permission> permissionResult = await this._permissionRepository.GetByIdAsync(id, true);
            if (permissionResult.IsFailed)
            {
                this._logger.LogError("Error updating permission: {Error}", permissionResult.Errors);
                return Result.Fail<Permission>(new Error($"Permission with ID {id} not found")
                        .WithMetadata(Constants.REASON, ErrorType.NotFound))
                    .WithError(permissionResult.Errors[0].Message);
            }

            Permission permission = permissionResult.Value;
            permission.Name = updateDto.Name;
            permission.Description = updateDto.Description;
            permission.PermissionString = updateDto.PermissionString;

            if (updateDto.WorkItemIds != null)
            {
                var existingAssignments = permission.PermissionWorkItems.ToList();
                foreach (PermissionWorkItem existingAssignment in existingAssignments)
                {
                    await this._permissionWorkItemRepository.DeleteAsync(existingAssignment);
                }

                if (updateDto.WorkItemIds.Any())
                {
                    foreach (long workItemId in updateDto.WorkItemIds)
                    {
                        PermissionWorkItem permissionWorkItem = new PermissionWorkItem
                        {
                            PermissionId = permission.Id,
                            WorkItemId = workItemId
                        };
                        await this._permissionWorkItemRepository.CreateAsync(permissionWorkItem);
                    }
                }
            }

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
                if (await this.CheckRolePermissionsAsync(userRole, request))
                {
                    return true;
                }
            }

            this._logger.LogInformation("No direct permission found for user {UserId} on resource {ResourceId}",
                request.UserId, request.ResourceId);
            return false;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error checking direct permissions for user {UserId} on resource {ResourceId}",
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

    private Result<Permission> validateCreatePermission(CreatePermissionDto permissionDto)
    {
        if (string.IsNullOrWhiteSpace(permissionDto.Name))
        {
            Result<Permission> result = Result.Fail(new Error("Name is required")
                .WithMetadata(Constants.REASON, ErrorType.ValidationError));
            return result;
        }

        Result<bool> permissionStringValidation =
            PermissionUtilities.ValidatePermissionString(permissionDto.PermissionString);

        if (permissionStringValidation.IsFailed)
        {
            Result<Permission> result = Result.Fail<Permission>(new Error("Permission string is invalid")
                .WithMetadata(Constants.REASON, ErrorType.ValidationError));
            return result;
        }

        return Result.Ok();
    }

    private async Task<bool> CheckRolePermissionsAsync(UserRole userRole, PermissionRequestDto request)
    {
        Result<IEnumerable<RolePermission>> result =
            await this._rolePermissionRepository.GetByRoleIdAsync(userRole.RoleId);

        if (result.IsFailed)
        {
            this._logger.LogWarning("Warning: Could not get role permissions for role {RoleId}", userRole.RoleId);
            return false;
        }

        foreach (RolePermission rolePermission in result.Value)
        {
            Result<Permission> permissionResult =
                await this._permissionRepository.GetByIdAsync(rolePermission.PermissionId);

            if (permissionResult.IsSuccess && await this.CheckResourcePermissionAsync(permissionResult.Value, request))
            {
                return true;
            }
        }

        return false;
    }

    private async Task<bool> CheckResourcePermissionAsync(Permission permission, PermissionRequestDto request)
    {
        Result<PermissionWorkItem> permissionWorkItemResult =
            await this._permissionWorkItemRepository.GetByCompositeKeyAsync(permission.Id, request.ResourceId);

        return permissionWorkItemResult is { IsSuccess: true, Value: not null } &&
               PermissionUtilities.DoesPermissionGrantAccess(
                   PermissionUtilities.StringToPermissionFlag(permission.PermissionString),
                   request.RequestedPermission);
    }
}
