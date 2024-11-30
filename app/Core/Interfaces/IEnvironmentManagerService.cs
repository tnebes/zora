using zora.Core.Enums;

namespace zora.Core.Interfaces;

public interface IEnvironmentManagerService
{
    EnvironmentType CurrentEnvironment { get; }
    bool IsDevelopment();
    bool IsProduction();
}
