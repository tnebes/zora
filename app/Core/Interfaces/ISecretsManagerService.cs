namespace zora.Services.Configuration;

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
}
