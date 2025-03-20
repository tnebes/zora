namespace zora.Core.Enums;

[Flags]
public enum PermissionFlag
{
    None = 0, // 00000
    Read = 1, // 00001
    Write = 2, // 00010
    Create = 4, // 00100
    Delete = 8, // 01000
    Admin = 16 // 10000
}

public static class PermissionFlagExtensions
{
    public static string GetPermissionName(this PermissionFlag permissionFlag) => permissionFlag.ToString();

    public static int GetPermissionValue(this PermissionFlag permissionFlag) => (int)permissionFlag;

    public static string GetPermissionMask(this PermissionFlag permissionFlag)
    {
        // fix for the none permission flag because it is 0 and log2(0) is undefined
        // hehe i love maths
        if (permissionFlag == PermissionFlag.None)
        {
            return "00000";
        }

        int position = (int)Math.Log2((int)permissionFlag);
        char[] mask = new char[5];
        Array.Fill(mask, '0');
        mask[position] = '1';
        return new string(mask);
    }
}
