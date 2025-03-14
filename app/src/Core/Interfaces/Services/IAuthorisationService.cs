#region

using System.Security.Claims;
using zora.Core.DTOs;
using zora.Core.DTOs.Permissions;

#endregion

namespace zora.Core.Interfaces.Services;

public interface IAuthorisationService
{
    Task<bool> IsAuthorisedAsync(PermissionRequestDto? permissionRequest);
    ValidationResult ValidateRequestAndClaims(PermissionRequestDto? permissionRequest, ClaimsPrincipal user);
}
