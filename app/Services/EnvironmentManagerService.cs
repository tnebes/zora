using Serilog;
using zora.Common.Attributes;
using zora.Common.Enums;
using zora.Common.Interfaces;

namespace zora.Services;

[ServiceLifetime(ServiceLifetime.Singleton)]
public class EnvironmentManagerService : IEnvironmentManagerService, IZoraService
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
