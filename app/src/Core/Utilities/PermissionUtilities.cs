#region

using FluentResults;
using zora.Core.Enums;

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
        PermissionFlag effectivePermissions = userPermissions;

        // If user has WRITE, CREATE, DELETE or ADMIN, they automatically have READ
        if ((userPermissions &
             (PermissionFlag.Write | PermissionFlag.Create | PermissionFlag.Delete | PermissionFlag.Admin)) != 0)
        {
            effectivePermissions |= PermissionFlag.Read;
        }

        return (effectivePermissions & requiredPermission) == requiredPermission;
    }

    public static PermissionFlag CombinePermissions(params PermissionFlag[] permissions)
    {
        PermissionFlag result = PermissionFlag.None;
        foreach (PermissionFlag permission in permissions)
        {
            result |= permission;
        }

        return result;
    }

    public static string CombinePermissionStrings(params string[] permissionStrings)
    {
        int result = 0;
        foreach (string permissionString in permissionStrings)
        {
            if (ValidatePermissionString(permissionString).IsSuccess)
            {
                result |= Convert.ToInt32(permissionString, 2);
            }
        }

        return Convert.ToString(result, 2).PadLeft(5, '0');
    }

    public static PermissionFlag GetEffectivePermissions(PermissionFlag permissions)
    {
        PermissionFlag effectivePermissions = permissions;

        // If user has WRITE, CREATE, DELETE or ADMIN, they automatically have READ
        if ((permissions &
             (PermissionFlag.Write | PermissionFlag.Create | PermissionFlag.Delete | PermissionFlag.Admin)) != 0)
        {
            effectivePermissions |= PermissionFlag.Read;
        }

        return effectivePermissions;
    }
}
