using zora.Common.Enums;

namespace zora.Services.Configuration
{
    public interface ISecretsManagerService
    {
        /// <summary>
        /// Gets a secret value by its key.
        /// </summary>
        /// <param name="key">The configuration key of the secret.</param>
        /// <returns>The secret value.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the secret is not found.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the key is null or empty.</exception>
        string GetSecret(string key);

        /// <summary>
        /// Gets the current environment type.
        /// </summary>
        EnvironmentType CurrentEnvironment { get; }
    }

    public class SecretsManagerService : ISecretsManagerService
    {
        private readonly IConfiguration _configuration;
        private readonly EnvironmentType _environmentType;

        public SecretsManagerService(IConfiguration configuration, string environment)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            if (string.IsNullOrEmpty(environment))
            {
                throw new ArgumentNullException(nameof(environment));
            }

            _environmentType = Enum.Parse<EnvironmentType>(environment, true);
        }

        public EnvironmentType CurrentEnvironment => _environmentType;

        public string GetSecret(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            string value = _configuration[key];
            if (string.IsNullOrEmpty(value))
            {
                throw new KeyNotFoundException($"Secret with key '{key}' not found.");
            }

            return value;
        }
    }
}
