namespace zora.Core.Enums;

[Flags]
public enum PermissionFlag
{
    None = 0,        // 00000
    Read = 1,        // 00001
    Write = 2,       // 00010
    Create = 4,      // 00100
    Delete = 8,      // 01000
    Admin = 16       // 10000
}
