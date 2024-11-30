using zora.Common.Enums;

namespace zora.Services;

public interface IEnvironmentManagerService
{
    EnvironmentType CurrentEnvironment { get; }
    bool IsDevelopment();
    bool IsProduction();
}
