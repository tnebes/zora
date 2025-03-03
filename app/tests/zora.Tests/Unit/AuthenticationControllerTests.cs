#region

using System.Reflection;
using System.Security.Claims;
using AutoMapper;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using zora.API.Controllers;
using zora.Core;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;
using zora.Core.Enums;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.Tests.Unit;

public sealed class AuthenticationControllerTests
{
    private readonly AuthenticationController _controller;
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<ILogger<AuthenticationController>> _mockLogger;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IUserService> _mockUserService;

    public AuthenticationControllerTests()
    {
        this._mockAuthService = new Mock<IAuthenticationService>();
        this._mockLogger = new Mock<ILogger<AuthenticationController>>();
        this._mockJwtService = new Mock<IJwtService>();
        this._mockUserService = new Mock<IUserService>();
        this._mockMapper = new Mock<IMapper>();

        this._controller = new AuthenticationController(this._mockAuthService.Object, this._mockLogger.Object,
            this._mockJwtService.Object, this._mockUserService.Object, this._mockMapper.Object
        );

        DefaultHttpContext httpContext = new DefaultHttpContext();
        this._controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact(DisplayName = "GIVEN a valid login request WHEN the token is requested THEN a token is returned")]
    public async Task Authenticate_WithValidCredentials_ReturnsOkWithToken()
    {
        LoginRequestDto loginRequest = new LoginRequestDto
        {
            Username = "testuser",
            Password = "password123"
        };

        User user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com"
        };

        string token = "valid-jwt-token";
        int expiresIn = 3600;

        this._mockAuthService.Setup(s => s.IsValidLoginRequest(It.IsAny<LoginRequestDto>()))
            .Returns(true);
        this._mockAuthService.Setup(s => s.AuthenticateUser(It.IsAny<LoginRequestDto>()))
            .ReturnsAsync(Result.Ok(user));
        this._mockJwtService.Setup(s => s.GenerateToken(It.IsAny<User>()))
            .Returns(token);
        this._mockJwtService.Setup(s => s.GetTokenExpiration())
            .Returns(expiresIn);

        ActionResult<TokenResponseDto> result = await this._controller.Authenticate(loginRequest);

        OkObjectResult? okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        TokenResponseDto? returnValue = okResult.Value.Should().BeOfType<TokenResponseDto>().Subject;
        returnValue.Token.Should().Be(token);
        returnValue.ExpiresIn.Should().Be(expiresIn);
    }

    [Fact(DisplayName = "GIVEN an invalid login request WHEN the token is requested THEN an error is returned")]
    public async Task Authenticate_WithInvalidCredentials_ReturnsBadRequest()
    {
        LoginRequestDto loginRequest = new LoginRequestDto
        {
            Username = "testuser",
            Password = "wrongpassword"
        };

        Error error = new Error("Invalid credentials");
        error.Metadata.Add(Constants.ERROR_TYPE, AuthenticationErrorType.InvalidCredentials);

        this._mockAuthService.Setup(s => s.IsValidLoginRequest(It.IsAny<LoginRequestDto>()))
            .Returns(true);
        this._mockAuthService.Setup(s => s.AuthenticateUser(It.IsAny<LoginRequestDto>()))
            .ReturnsAsync(Result.Fail(error));

        ActionResult<TokenResponseDto> result = await this._controller.Authenticate(loginRequest);

        UnauthorizedObjectResult? unauthorizedResult =
            result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact(DisplayName =
        "GIVEN a login request with empty username or empty password WHEN the token is requested THEN bad request is returned")]
    public async Task Authenticate_WithEmptyCredentials_ReturnsBadRequest()
    {
        LoginRequestDto loginRequest = new LoginRequestDto
        {
            Username = "",
            Password = "password123"
        };

        this._mockAuthService.Setup(s => s.IsValidLoginRequest(It.IsAny<LoginRequestDto>()))
            .Returns(false);

        ActionResult<TokenResponseDto> result = await this._controller.Authenticate(loginRequest);

        BadRequestObjectResult? badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact(DisplayName =
        "GIVEN a login request for a deleted user WHEN the token is requested THEN internal server error is returned")]
    public async Task Authenticate_WithDeletedUser_ReturnsBadRequest()
    {
        LoginRequestDto loginRequest = new LoginRequestDto
        {
            Username = "deleteduser",
            Password = "password123"
        };

        Error error = new Error("User is deleted or inactive");
        error.Metadata.Add(Constants.ERROR_TYPE, AuthenticationErrorType.UserDeleted);

        this._mockAuthService.Setup(s => s.IsValidLoginRequest(It.IsAny<LoginRequestDto>()))
            .Returns(true);
        this._mockAuthService.Setup(s => s.AuthenticateUser(It.IsAny<LoginRequestDto>()))
            .ReturnsAsync(Result.Fail(error));

        ActionResult<TokenResponseDto> result = await this._controller.Authenticate(loginRequest);

        UnauthorizedObjectResult? unauthorizedResult =
            result.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact(DisplayName =
        "GIVEN a logged in user WHEN getcurrentuser is called THEN valid information about the user is returned")]
    public async Task GetCurrentUser_WithAuthenticatedUser_ReturnsUserInfo()
    {
        long userId = 1;
        User user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com"
        };

        MinimumUserDto userDto = new MinimumUserDto
        {
            Id = userId,
            Username = "testuser"
        };

        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "Test"));

        DefaultHttpContext httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        this._controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        this._mockUserService.Setup(s => s.GetByIdAsync(userId, false))
            .ReturnsAsync(Result.Ok(user));
        this._mockMapper.Setup(m => m.Map<MinimumUserDto>(user))
            .Returns(userDto);

        ActionResult<MinimumUserDto> result = await this._controller.GetCurrentUser();

        OkObjectResult? okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        MinimumUserDto? returnValue = okResult.Value.Should().BeOfType<MinimumUserDto>().Subject;
        returnValue.Id.Should().Be(userId);
        returnValue.Username.Should().Be("testuser");
    }

    [Fact(DisplayName = "GIVEN an anonymous user WHEN getcurrentuser is called THEN unauthorized is returned")]
    public void GetCurrentUser_WithAnonymousUser_ReturnsUnauthorized()
    {
        Type controllerType = typeof(AuthenticationController);
        MethodInfo methodInfo = controllerType.GetMethod("GetCurrentUser");

        object[] attributes = methodInfo.GetCustomAttributes(typeof(AuthorizeAttribute), true);

        attributes.Should().NotBeEmpty();
        attributes[0].Should().BeOfType<AuthorizeAttribute>();
    }
}
