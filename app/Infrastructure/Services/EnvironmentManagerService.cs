using Serilog;
using zora.Core.Attributes;
using zora.Core.Enums;
using zora.Core.Interfaces;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Singleton)]
public sealed class EnvironmentManagerService : IEnvironmentManagerService, IZoraService
{
    public EnvironmentType CurrentEnvironment { get; }
    private readonly ILogger<EnvironmentManagerService> _logger;

    public EnvironmentManagerService(IConfiguration configuration, ILogger<EnvironmentManagerService> logger)
    {
        this._logger = logger;
        _ = configuration ?? throw new ArgumentNullException(nameof(configuration));
        string? environment = configuration["Environment"];
        if (string.IsNullOrEmpty(environment))
        {
            this._logger.LogError("Environment not found in configuration.");
            throw new KeyNotFoundException("Environment not found in configuration.");
        }

        this.CurrentEnvironment = Enum.Parse<EnvironmentType>(environment, true);
    }

    public bool IsDevelopment() => this.CurrentEnvironment.IsDevelopment();

    public bool IsProduction() => this.CurrentEnvironment.IsProduction();
}
