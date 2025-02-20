#region

using System.ComponentModel;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zora.API.Extensions;
using zora.Core;
using zora.Core.Domain;
using zora.Core.DTOs;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;
using zora.Core.Enums;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.API.Controllers;

/// <summary>
///     Controller for handling user authentication.
///     Provides endpoints for token generation and authentication status checks.
/// </summary>
[ApiController]
[Route("api/v1/authentication")]
[Produces("application/json")]
[Consumes("application/json")]
[Description("Authentication API")]
public sealed class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthenticationController> _logger;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;

    public AuthenticationController(
        IAuthenticationService authenticationService,
        ILogger<AuthenticationController> logger,
        IJwtService jwtService,
        IUserService userService,
        IMapper mapper)
    {
        this._authenticationService = authenticationService;
        this._logger = logger;
        this._jwtService = jwtService;
        this._userService = userService;
        this._mapper = mapper;
    }

    /// <summary>\
    ///     Authenticates a user and returns a JWT token if successful.
    /// </summary>
    /// <param name="login">Login credentials including username and password</param>
    /// <returns>TokenResponseDto containing JWT token and user information</returns>
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Authentication")]
    [Description("Authenticates a user and returns a JWT token if successful.")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponseDto>> Authenticate([FromBody] LoginRequestDto login)
    {
        try
        {
            if (!this._authenticationService.IsValidLoginRequest(login))
            {
                return this.BadRequest(new { Message = "Invalid login request format" });
            }

            Result<User> userResult = await this._authenticationService.AuthenticateUser(login);

            if (userResult.IsFailed)
            {
                string? ipAddress = this.HttpContext.Connection.RemoteIpAddress?.ToString();
                this._logger.LogInformation("User {Username} ({IpAddress}) failed to authenticate.",
                    login.Username, ipAddress);

                IError error = userResult.Errors[0];
                AuthenticationErrorType errorType = error.Metadata.TryGetValue(Constants.ERROR_TYPE, out object? value)
                    ? (AuthenticationErrorType)(value ?? AuthenticationErrorType.AuthenticationError)
                    : AuthenticationErrorType.AuthenticationError;

                return errorType switch
                {
                    AuthenticationErrorType.UserAlreadyAuthenticated =>
                        this.BadRequest(new { error.Message }),
                    AuthenticationErrorType.InvalidCredentials =>
                        this.Unauthorized(new { error.Message }),
                    _ => this.StatusCode(StatusCodes.Status500InternalServerError,
                        new { Message = Constants.ERROR_500_MESSAGE })
                };
            }

            string jwt = this._jwtService.GenerateToken(userResult.Value);

            this._logger.LogInformation("User {Username} authenticated successfully.", login.Username);

            return this.Ok(new TokenResponseDto
            {
                Token = jwt,
                ExpiresIn = this._jwtService.GetTokenExpiration()
            });
        }
        catch (Exception e)
        {
            this._logger.LogError(e, "Error authenticating user.");
            return this.StatusCode(StatusCodes.Status500InternalServerError,
                new { Message = Constants.ERROR_500_MESSAGE });
        }
    }

    [HttpGet("check")]
    [ProducesResponseType(typeof(AuthenticationStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Authentication")]
    [Description("Check if the user is authenticated")]
    [Authorize]
    public async Task<ActionResult<AuthenticationStatusDto>> CheckAuthStatus()
    {
        try
        {
            long userId = this.HttpContext.User.GetUserId();
            Result<User> user = await this._userService.GetByIdAsync(userId);

            // TODO ugly as sin
            if (user.IsFailed)
            {
                IError error = user.Errors[0];
                ErrorType errorType = error.Metadata.TryGetValue("errorType", out object? value)
                    ? (ErrorType)(value ?? ErrorType.SystemError)
                    : ErrorType.SystemError;

                return errorType switch
                {
                    ErrorType.NotFound => this.NotFound(error.Message),
                    _ => this.StatusCode(500, Constants.ERROR_500_MESSAGE)
                };
            }

            return this.Ok(this._mapper.Map<AuthenticationStatusDto>(user.Value));
        }
        catch (Exception e)
        {
            this._logger.LogError(e, "Error checking authentication status");
            return this.StatusCode(500, Constants.ERROR_500_MESSAGE);
        }
    }

    [HttpGet("current-user")]
    [ProducesResponseType(typeof(MinimumUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Authentication")]
    [Description("Returns minimal information about the current user")]
    [Authorize]
    public async Task<ActionResult<MinimumUserDto>> GetCurrentUser()
    {
        try
        {
            long userId = this.HttpContext.User.GetUserId();
            Result<User> user = await this._userService.GetByIdAsync(userId);

            // TODO ugly as sin
            if (user.IsFailed)
            {
                IError error = user.Errors[0];
                ErrorType errorType = error.Metadata.TryGetValue("errorType", out object? value)
                    ? (ErrorType)(value ?? ErrorType.SystemError)
                    : ErrorType.SystemError;

                return errorType switch
                {
                    ErrorType.NotFound => this.NotFound(error.Message),
                    _ => this.StatusCode(404, Constants.ERROR_404_MESSAGE)
                };
            }

            return this.Ok(this._mapper.Map<MinimumUserDto>(user.Value));
        }
        catch (Exception e)
        {
            this._logger.LogError(e, "Error getting current user");
            return this.StatusCode(500, Constants.ERROR_500_MESSAGE);
        }
    }
}
