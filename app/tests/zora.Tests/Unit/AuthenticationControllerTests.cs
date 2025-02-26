#region

using AutoMapper;
using FluentAssertions;
using FluentResults;
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
    }

    [Fact]
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

    [Fact]
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

        ObjectResult? objectResult = result.Result.Should().BeAssignableTo<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}
