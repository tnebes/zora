#region

using System.Security.Claims;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using zora.API.Extensions;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs;
using zora.Core.DTOs.Requests;
using zora.Core.Enums;
using zora.Core.Interfaces.Services;
using zora.Core.Requirements;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public sealed class AuthorisationService : IAuthorizationHandler, IAuthorisationService, IZoraService
{
    private const string CacheKeyPrefix = "auth_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private readonly IMemoryCache _cache;
    private readonly ILogger<AuthorisationService> _logger;
    private readonly IPermissionService _permissionService;
    private readonly IUserRoleService _userRoleService;
    private readonly IWorkItemService _workItemService;

    public AuthorisationService(
        ILogger<AuthorisationService> logger,
        IPermissionService permissionService,
        IWorkItemService workItemService,
        IUserRoleService userRoleService,
        IMemoryCache cache)
    {
        this._logger = logger;
        this._permissionService = permissionService;
        this._workItemService = workItemService;
        this._userRoleService = userRoleService;
        this._cache = cache;
    }

    public async Task<bool> IsAuthorisedAsync(PermissionRequestDto? permissionRequest)
    {
        if (permissionRequest == null)
        {
            this._logger.LogWarning("Permission request is null");
            return false;
        }

        if (await this._userRoleService.IsAdminAsync(permissionRequest.UserId))
        {
            return true;
        }

        string cacheKey = AuthorisationService.GetCacheKey(permissionRequest);

        if (this._cache.TryGetValue(cacheKey, out bool cachedResult))
        {
            this._logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cachedResult;
        }

        this._logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);

        try
        {
            bool isAuthorized = await this.CheckAuthorizationAsync(permissionRequest);
            this._cache.Set(cacheKey, isAuthorized, AuthorisationService.CacheDuration);
            return isAuthorized;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error checking authorization for user {UserId} on resource {ResourceId}",
                permissionRequest.UserId, permissionRequest.ResourceId);
            return false;
        }
    }

    public ValidationResult ValidateRequestAndClaims(PermissionRequestDto? permissionRequest, ClaimsPrincipal user)
    {
        if (permissionRequest == null)
        {
            this._logger.LogWarning("Permission request is null");
            return ValidationResult.Fail("Permission request cannot be null", StatusCodes.Status400BadRequest);
        }

        try
        {
            long userIdClaim = user.GetUserId();
            if (userIdClaim != permissionRequest.UserId)
            {
                this._logger.LogWarning("User ID mismatch. Token ID: {TokenId}, Request ID: {RequestId}",
                    userIdClaim, permissionRequest.UserId);
                return ValidationResult.Fail("User ID in request does not match authenticated user",
                    StatusCodes.Status403Forbidden);
            }

            return ValidationResult.Success();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting user ID from claims");
            return ValidationResult.Fail("Error getting user ID from claims", StatusCodes.Status400BadRequest);
        }
    }

    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        foreach (IAuthorizationRequirement requirement in context.Requirements)
        {
            if (requirement is not WorkItemPermissionRequirement workItemRequirement ||
                context.User.Identity?.IsAuthenticated != true)
            {
                continue;
            }

            try
            {
                long userId = context.User.GetUserId();
                PermissionRequestDto permissionRequest = new()
                {
                    UserId = userId,
                    ResourceId = workItemRequirement.WorkItemId,
                    RequestedPermission = workItemRequirement.RequiredPermission
                };

                ValidationResult validationResult = this.ValidateRequestAndClaims(permissionRequest, context.User);
                if (!validationResult.IsValid)
                {
                    this._logger.LogWarning("Validation failed: {ErrorMessage}", validationResult.ErrorMessage);
                    continue;
                }

                if (await this.IsAuthorisedAsync(permissionRequest))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    this._logger.LogInformation(
                        "Authorization denied for user {UserId} on work item {WorkItemId} with permission {Permission}",
                        userId, workItemRequirement.WorkItemId, workItemRequirement.RequiredPermission);
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error during authorization handling");
            }
        }
    }

    private static string GetCacheKey(PermissionRequestDto request) =>
        $"{AuthorisationService.CacheKeyPrefix}{request.UserId}_{request.ResourceId}_{request.RequestedPermission}";

    private async Task<bool> CheckAuthorizationAsync(PermissionRequestDto request) =>
        await this._permissionService.HasDirectPermissionAsync(request) ||
        await this.CheckAncestorPermissionsAsync(request);

    private async Task<bool> CheckAncestorPermissionsAsync(PermissionRequestDto request)
    {
        Result<WorkItemType> workItemTypeResult = await this._workItemService.GetWorkItemType(request.ResourceId);
        if (workItemTypeResult.IsFailed)
        {
            this._logger.LogError("Failed to retrieve work item type for resource {ResourceId}", request.ResourceId);
            return false;
        }

        this._logger.LogDebug("Retrieved work item type {WorkItemType} for resource {ResourceId}",
            workItemTypeResult.Value, request.ResourceId);

        WorkItem? ancestor = await this.GetAncestorByType(request.ResourceId, workItemTypeResult.Value);
        if (ancestor == null)
        {
            this._logger.LogInformation("No ancestor found for resource {ResourceId} of type {WorkItemType}",
                request.ResourceId, workItemTypeResult.Value);
            return false;
        }

        this._logger.LogDebug("Retrieved ancestor {AncestorId} for resource {ResourceId} of type {WorkItemType}",
            ancestor.Id, request.ResourceId, workItemTypeResult.Value);

        PermissionRequestDto ancestorRequest = new()
        {
            UserId = request.UserId,
            ResourceId = ancestor.Id,
            RequestedPermission = request.RequestedPermission
        };

        if (await this.IsAuthorisedAsync(ancestorRequest))
        {
            this._logger.LogInformation(
                "Authorised for user {UserId} on ancestor {AncestorId} of resource {ResourceId} with permission {Permission}",
                request.UserId, ancestor.Id, request.ResourceId, request.RequestedPermission);
            return true;
        }

        this._logger.LogInformation(
            "Not authorised for user {UserId} on ancestor {AncestorId} of resource {ResourceId} with permission {Permission}",
            request.UserId, ancestor.Id, request.ResourceId, request.RequestedPermission);

        if (ancestor is Project { ProgramId: not null } project)
        {
            this._logger.LogDebug("Checking ancestor permissions for program {ProgramId}", project.ProgramId.Value);
            return await this.CheckAncestorPermissionsAsync(new PermissionRequestDto
            {
                UserId = request.UserId,
                ResourceId = project.ProgramId.Value,
                RequestedPermission = request.RequestedPermission
            });
        }

        this._logger.LogDebug(
            "Not authorised for user {UserId} on ancestor {AncestorId} of resource {ResourceId} with permission {Permission}",
            request.UserId, ancestor.Id, request.ResourceId, request.RequestedPermission);
        return false;
    }

    private async Task<WorkItem?> GetAncestorByType(long resourceId, WorkItemType workItemType)
    {
        return workItemType switch
        {
            WorkItemType.Task => await this.GetAncestorResult<Project>(resourceId),
            WorkItemType.Project => await this.GetAncestorResult<ZoraProgram>(resourceId),
            WorkItemType.Program => null,
            _ => null
        };
    }

    private async Task<WorkItem?> GetAncestorResult<T>(long resourceId) where T : WorkItem
    {
        Result<T> result = await this._workItemService.GetNearestAncestorOf<T>(resourceId);
        if (result.IsFailed)
        {
            this._logger.LogError("Failed to retrieve ancestor for resource {ResourceId}", resourceId);
            return null;
        }

        return result.Value;
    }
}
