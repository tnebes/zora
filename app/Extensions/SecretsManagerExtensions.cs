using zora.Services.Configuration;

namespace zora.Extensions;

public static class SecretsManagerExtensions
{
    public static IServiceCollection AddSecretsManager(this IServiceCollection services, IConfiguration configuration)
    {
        string environment = configuration["Environment"];
        if (string.IsNullOrEmpty(environment))
        {
            throw new ArgumentNullException(nameof(environment));
        }

        services.AddSingleton<ISecretsManagerService>(new SecretsManagerService(configuration, environment));
        return services;
    }
}
