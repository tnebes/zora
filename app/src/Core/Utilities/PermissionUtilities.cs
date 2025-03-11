#region

using FluentResults;
using zora.Core.Enums;

#endregion

namespace zora.Core.Utilities;

public static class PermissionUtilities
{
    public static bool DoesPermissionGrantAccess(string permissionString, PermissionFlag requestedPermission)
    {
        int permissions = Convert.ToInt32(permissionString, 2);
        return (permissions & (int)requestedPermission) == (int)requestedPermission;
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

        if (permissionString.All(c => c == '0'))
        {
            return Result.Fail<bool>("Permission string cannot be all 0s");
        }

        return Result.Ok(true);
    }
}
