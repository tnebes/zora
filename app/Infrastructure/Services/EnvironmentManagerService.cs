using Serilog;
using zora.Core.Attributes;
using zora.Core.Enums;
using zora.Core.Interfaces;

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Singleton)]
public sealed class EnvironmentManagerService : IEnvironmentManagerService, IZoraService
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
