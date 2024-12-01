#region

using System.Text;
using Microsoft.Win32.SafeHandles;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs;
using zora.Core.Interfaces;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        this._userRepository = userRepository;
        this._logger = logger;
    }

    public async Task<bool> ValidateUser(LoginRequest login)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(login.Username) || string.IsNullOrWhiteSpace(login.Password))
            {
                return false;
            }

            User? user = await this._userRepository.GetUserByUsernameAsync(login.Username);
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
