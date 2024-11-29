namespace zora.Common.Enums
{
    public enum EnvironmentType
    {
        Development,
        Production
    }

    public static class EnvironmentTypeExtensions
    {
        public static bool IsDevelopment(this EnvironmentType environmentType)
        {
            return environmentType == EnvironmentType.Development;
        }

        public static bool IsProduction(this EnvironmentType environmentType)
        {
            return environmentType == EnvironmentType.Production;
        }
    }
}