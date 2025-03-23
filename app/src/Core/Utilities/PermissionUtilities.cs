#region

using FluentResults;
using zora.Core.Enums;
using System.Linq;

#endregion

namespace zora.Core.Utilities;

public static class PermissionUtilities
{
    public static bool DoesPermissionGrantAccess(PermissionFlag providedPermission, PermissionFlag requestedPermission)
    {
        Result<bool> validationResult = ValidatePermissionString(providedPermission.GetPermissionMask());
        if (validationResult.IsFailed)
        {
            return false;
        }

        return HasPermission(providedPermission, requestedPermission);
    }

    public static Result<bool> ValidatePermissionString(string permissionString)
    {
        if (string.IsNullOrWhiteSpace(permissionString))
        {
            return Result.Fail<bool>("Permission string is required");
        }

        if (!permissionString.All(c => c is '0' or '1') ||
            permissionString.Length != 5)
        {
            return Result.Fail<bool>("Permission string must be 5 characters long and contain only 0s and 1s");
        }

        return Result.Ok(true);
    }

    public static string PermissionFlagToString(PermissionFlag permissionFlag) =>
        Convert.ToString((int)permissionFlag, 2).PadLeft(5, '0');

    public static PermissionFlag StringToPermissionFlag(string permissionString)
    {
        Result<bool> validationResult = ValidatePermissionString(permissionString);
        if (validationResult.IsFailed)
        {
            return PermissionFlag.None;
        }

        return (PermissionFlag)Convert.ToInt32(permissionString, 2);
    }

    public static bool HasPermission(PermissionFlag userPermissions, PermissionFlag requiredPermission)
    {
        // If user has Admin permission, automatically grant any permission
        if ((userPermissions & PermissionFlag.Admin) != 0)
        {
            return true;
        }

        // Calculate effective permissions (add READ if user has any higher permission)
        PermissionFlag effectivePermissions = userPermissions;
        if ((userPermissions & (PermissionFlag.Write | PermissionFlag.Create | PermissionFlag.Delete)) != 0)
        {
            effectivePermissions |= PermissionFlag.Read;
        }

        // Permission check: all bits in requiredPermission must be present in effectivePermissions
        return (effectivePermissions & requiredPermission) == requiredPermission;
    }

    public static PermissionFlag CombinePermissions(params PermissionFlag[] permissions)
    {
        // Simply OR all permission flags together
        return permissions.Aggregate(PermissionFlag.None, (current, permission) => current | permission);
    }

    public static string CombinePermissionStrings(params string[] permissionStrings)
    {
        return permissionStrings
            .Where(ps => ValidatePermissionString(ps).IsSuccess)
            .Select(ps => Convert.ToInt32(ps, 2))
            .Aggregate(0, (current, value) => current | value)
            .ToString("D5")
            .PadLeft(5, '0');
    }

    public static PermissionFlag GetEffectivePermissions(PermissionFlag permissions)
    {
        // If Admin permission is present, return all permissions
        if ((permissions & PermissionFlag.Admin) != 0)
        {
            return PermissionFlag.Read | PermissionFlag.Write | PermissionFlag.Create | PermissionFlag.Delete | PermissionFlag.Admin;
        }

        // Add READ if any higher-level permission is present
        PermissionFlag effectivePermissions = permissions;
        if ((permissions & (PermissionFlag.Write | PermissionFlag.Create | PermissionFlag.Delete)) != 0)
        {
            effectivePermissions |= PermissionFlag.Read;
        }

        return effectivePermissions;
    }
}
