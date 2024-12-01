#region

using zora.Core.Enums;

#endregion

namespace zora.Core.Interfaces;

public interface IEnvironmentManagerService
{
    EnvironmentType CurrentEnvironment { get; }
    bool IsDevelopment();
    bool IsProduction();
}
