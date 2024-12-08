#region

using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs;
using zora.Core.Interfaces;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public sealed class UserService : IUserService, IZoraService
{
    private readonly ILogger<UserService> _logger;
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        this._userRepository = userRepository;
        this._logger = logger;
    }

    public async Task<bool> ValidateUser(LoginRequestDto login)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(login.Username) || string.IsNullOrWhiteSpace(login.Password))
            {
                return false;
            }

            User? user = await this._userRepository.GetByUsernameAsync(login.Username);
            if (user == null)
            {
                return false;
            }

            string hashedPassword = this.HashPassword(login.Password);
            return BCrypt.Net.BCrypt.Verify(login.Password, hashedPassword);
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error validating user {Username}", login.Username);
            throw;
        }
    }

    private string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);
}
