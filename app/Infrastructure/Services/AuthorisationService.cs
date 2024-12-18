#region

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using zora.API.Extensions;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs;
using zora.Core.DTOs.Requests;
using zora.Core.Enums;
using zora.Core.Interfaces;
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
    private readonly IWorkItemService _workItemService;

    public AuthorisationService(
        ILogger<AuthorisationService> logger,
        IPermissionService permissionService,
        IWorkItemService workItemService,
        IMemoryCache cache)
    {
        this._logger = logger;
        this._permissionService = permissionService;
        this._workItemService = workItemService;
        this._cache = cache;
    }

    public async Task<bool> IsAuthorisedAsync(PermissionRequestDto? permissionRequest)
    {
        if (permissionRequest == null)
        {
            this._logger.LogWarning("Permission request is null");
            return false;
        }

        string cacheKey =
            $"{AuthorisationService.CacheKeyPrefix}{permissionRequest.UserId}_{permissionRequest.ResourceId}_{permissionRequest.RequestedPermission}";

        if (this._cache.TryGetValue(cacheKey, out bool cachedResult))
        {
            return cachedResult;
        }

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

        long userIdClaim;
        try
        {
            userIdClaim = user.GetUserId();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting user ID from claims");
            return ValidationResult.Fail("Error getting user ID from claims", StatusCodes.Status400BadRequest);
        }

        if (userIdClaim != permissionRequest.UserId)
        {
            this._logger.LogWarning("User ID mismatch. Token ID: {TokenId}, Request ID: {RequestId}",
                userIdClaim, permissionRequest.UserId);
            return ValidationResult.Fail("User ID in request does not match authenticated user",
                StatusCodes.Status403Forbidden);
        }

        return ValidationResult.Success();
    }

    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        foreach (IAuthorizationRequirement requirement in context.Requirements)
        {
            if (requirement is not WorkItemPermissionRequirement workItemRequirement)
            {
                continue;
            }

            if (context.User.Identity?.IsAuthenticated != true)
            {
                continue;
            }

            long userId;
            try
            {
                userId = context.User.GetUserId();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error getting user ID from claims");
                continue;
            }

            PermissionRequestDto permissionRequest = new PermissionRequestDto
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

            try
            {
                bool isAuthorized = await this.IsAuthorisedAsync(permissionRequest);
                if (isAuthorized)
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
                this._logger.LogError(ex,
                    "Error during authorization for user {UserId} on work item {WorkItemId}",
                    userId, workItemRequirement.WorkItemId);
            }
        }
    }

    private async Task<bool> CheckAuthorizationAsync(PermissionRequestDto request)
    {
        if (await this._permissionService.HasDirectPermissionAsync(request))
        {
            return true;
        }

        return await this.CheckAncestorPermissionsAsync(request);
    }

    private async Task<bool> CheckAncestorPermissionsAsync(PermissionRequestDto request)
    {
        WorkItemType workItemType = await this._workItemService.GetWorkItemType(request.ResourceId);
        WorkItem? ancestor;

        switch (workItemType)
        {
            case WorkItemType.Task:
                ancestor = await this._workItemService.GetNearestAncestorOf<Project>(request.ResourceId);
                break;
            case WorkItemType.Project:
                ancestor = await this._workItemService.GetNearestAncestorOf<ZoraProgram>(request.ResourceId);
                break;
            case WorkItemType.Program:
                this._logger.LogWarning(
                    "Program is the top level ancestor for resource {ResourceId}",
                    request.ResourceId);
                return false;
            default:
                this._logger.LogError("Unknown work item type: {WorkItemType}", workItemType);
                return false;
        }

        if (ancestor == null)
        {
            this._logger.LogInformation(
                "No ancestor found for resource {ResourceId} of type {WorkItemType}",
                request.ResourceId,
                workItemType);
            return false;
        }

        PermissionRequestDto ancestorRequest = new()
        {
            UserId = request.UserId,
            ResourceId = ancestor.Id,
            RequestedPermission = request.RequestedPermission
        };

        bool isAuthorised = await this.IsAuthorisedAsync(ancestorRequest);

        if (isAuthorised)
        {
            this._logger.LogInformation(
                "Authorised for user {UserId} on ancestor {AncestorId} of resource {ResourceId} with permission {Permission}",
                request.UserId, ancestor.Id, request.ResourceId, request.RequestedPermission);
            return true;
        }

        if (ancestor is Project project && project.ProgramId.HasValue)
        {
            return await this.CheckAncestorPermissionsAsync(new PermissionRequestDto
            {
                UserId = request.UserId,
                ResourceId = project.ProgramId.Value,
                RequestedPermission = request.RequestedPermission
            });
        }

        this._logger.LogInformation(
            "Not authorised for user {UserId} on ancestor {AncestorId} of resource {ResourceId} with permission {Permission}",
            request.UserId, ancestor.Id, request.ResourceId, request.RequestedPermission);
        return false;
    }
}
