#region

using Serilog;
using zora.Core.Attributes;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.Infrastructure.Services.Configuration;

[ServiceLifetime(ServiceLifetime.Singleton)]
public sealed class SecretsManagerService : ISecretsManagerService, IZoraService
{
    private readonly IConfiguration _configuration;

    public SecretsManagerService(IConfiguration configuration) => this._configuration = configuration;

    public string GetSecret(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException(nameof(key));
        }

        string? value = this._configuration[key];

        if (string.IsNullOrEmpty(value))
        {
            string? errorMessage = $"Secret with key '{key}' not found. Use dotnet user-secrets.";
            Log.Error(errorMessage);
            throw new KeyNotFoundException(errorMessage);
        }

        return value;
    }
}
