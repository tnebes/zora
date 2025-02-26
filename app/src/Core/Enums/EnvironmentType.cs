namespace zora.Core.Enums;

public enum EnvironmentType
{
    Development,
    Production
}

public static class EnvironmentTypeExtensions
{
    public static bool IsDevelopment(this EnvironmentType environmentType) =>
        environmentType == EnvironmentType.Development;

    public static bool IsProduction(this EnvironmentType environmentType) =>
        environmentType == EnvironmentType.Production;
}
