using Serilog;
using zora.Common.Enums;

namespace zora.Services;

public class EnvironmentManagerService : IEnvironmentManagerService
{

    public EnvironmentType CurrentEnvironment { get; }

    public EnvironmentManagerService(IConfiguration configuration)
    {
        _ = configuration ?? throw new ArgumentNullException(nameof(configuration));
        var environment = configuration["Environment"];
        if (string.IsNullOrEmpty(environment))
        {
            Log.Error("Environment not found in configuration.");
            throw new KeyNotFoundException("Environment not found in configuration.");
        }

        this.CurrentEnvironment = Enum.Parse<EnvironmentType>(environment, true);
    }

    public bool IsDevelopment() => this.CurrentEnvironment.IsDevelopment();

    public bool IsProduction() => this.CurrentEnvironment.IsProduction();
}
