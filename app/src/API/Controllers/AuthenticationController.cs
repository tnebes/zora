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
using zora.Core.DTOs.Users;
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

    /// <summary>
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
            if (this.HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                this.LogUnauthorisedAccess(this.RouteData.Values["action"]?.ToString() ?? string.Empty);
                return this.BadRequest(new { Message = "User is already authenticated" });
            }

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
                    AuthenticationErrorType.UserDeleted =>
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

    /// <summary>
    ///     Checks if the user is authenticated and returns authentication status.
    /// </summary>
    /// <returns>AuthenticationStatusDto containing authentication status</returns>
    [HttpGet("check")]
    [ProducesResponseType(typeof(AuthenticationStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Tags("Authentication")]
    [Description("Check if the user is authenticated")]
    public async Task<ActionResult<AuthenticationStatusDto>> CheckAuthStatus()
    {
        try
        {
            if (!this.HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                this.LogUnauthorisedAccess(this.RouteData.Values["action"]?.ToString() ?? string.Empty);
                return this.Unauthorized(new { Message = "User is not authenticated" });
            }

            long userId = this.HttpContext.User.GetUserId();
            Result<User> userResult = await this._userService.GetByIdAsync(userId, true);

            if (userResult.IsFailed)
            {
                IError error = userResult.Errors[0];
                ErrorType errorType = error.Metadata.TryGetValue(Constants.ERROR_TYPE, out object? value)
                    ? (ErrorType)(value ?? ErrorType.SystemError)
                    : ErrorType.SystemError;

                return errorType == ErrorType.NotFound
                    ? this.NotFound(new { error.Message })
                    : this.StatusCode(StatusCodes.Status500InternalServerError,
                        new { Message = Constants.ERROR_500_MESSAGE });
            }

            return this.Ok(this._mapper.Map<AuthenticationStatusDto>(userResult.Value));
        }
        catch (Exception e)
        {
            this._logger.LogError(e, "Error checking authentication status");
            return this.StatusCode(StatusCodes.Status500InternalServerError,
                new { Message = Constants.ERROR_500_MESSAGE });
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
            if (!this.HttpContext.User.Identity.IsAuthenticated)
            {
                this.LogUnauthorisedAccess(this.RouteData.Values["action"]?.ToString() ?? string.Empty);
                return this.Unauthorized();
            }

            long userId;
            try
            {
                userId = this.HttpContext.User.GetUserId();

                if (userId <= 0)
                {
                    this.LogUnauthorisedAccess(this.RouteData.Values["action"]?.ToString() ?? string.Empty);
                    return this.Unauthorized();
                }
            }
            catch (UnauthorizedAccessException)
            {
                this.LogUnauthorisedAccess(this.RouteData.Values["action"]?.ToString() ?? string.Empty);
                return this.Unauthorized();
            }

            Result<User> userResult = await this._userService.GetByIdAsync(userId);

            if (userResult == null)
            {
                return this.StatusCode(500, Constants.ERROR_500_MESSAGE);
            }

            if (userResult.IsFailed)
            {
                if (userResult.Errors.Any(e => e.Message.Contains("not found")))
                {
                    return this.NotFound();
                }

                return this.StatusCode(500, Constants.ERROR_500_MESSAGE);
            }

            MinimumUserDto userDto = this._mapper.Map<MinimumUserDto>(userResult.Value);
            return this.Ok(userDto);
        }
        catch (Exception e)
        {
            this._logger.LogError(e, "Error retrieving current user");
            return this.StatusCode(StatusCodes.Status500InternalServerError,
                new { Message = Constants.ERROR_500_MESSAGE });
        }
    }

    private void LogUnauthorisedAccess(string endpoint)
    {
        string ipAddress = this.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
        string userAgent = this.HttpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown User-Agent";
        this._logger.LogWarning("Unauthorized access attempt from {IpAddress} to {Endpoint}. User-Agent: {UserAgent}",
            ipAddress, endpoint, userAgent);
    }
}
