#region

using System.Security.Claims;
using zora.Core.DTOs;
using zora.Core.DTOs.Requests;

#endregion

namespace zora.Core.Interfaces;

public interface IAuthorisationService
{
    Task<bool> IsAuthorisedAsync(PermissionRequestDto? permissionRequest);
    ValidationResult ValidateRequestAndClaims(PermissionRequestDto? permissionRequest, ClaimsPrincipal user);
}
