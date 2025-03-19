#region

using System.Security.Claims;
using zora.Core.Domain;
using zora.Core.DTOs;
using zora.Core.DTOs.Permissions;
using zora.Core.Enums;

#endregion

namespace zora.Core.Interfaces.Services;

public interface IAuthorisationService
{
    Task<bool> IsAuthorisedAsync(PermissionRequestDto? permissionRequest);
    ValidationResult ValidateRequestAndClaims(PermissionRequestDto? permissionRequest, ClaimsPrincipal user);

    Task<IQueryable<T>> FilterByPermission<T>(IQueryable<T> query, long userId, PermissionFlag permissionFlag)
        where T : WorkItem;
}
