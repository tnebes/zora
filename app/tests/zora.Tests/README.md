# Zora Test Suite

This document explains the structure and organization of the Zora test suite, focusing on the abstractions and helper
classes that make writing tests more maintainable and DRY (Don't Repeat Yourself).

## Test Structure

The test suite is organized into the following components:

### Base Classes

1. **BaseIntegrationTest**
    - Base class for all integration tests
    - Provides common setup and helper methods for HTTP requests and assertions
    - Handles client creation and authentication setup

2. **AuthenticationTestBase**
    - Extends BaseIntegrationTest for authentication-specific tests
    - Provides mock setup for authentication services
    - Contains helper methods for token requests and user authentication

### Helper Classes

1. **TestHelpers**
    - Static utility class with common test operations
    - HTTP request execution methods
    - Response assertion methods
    - DTO mapping utilities
    - Test fixture setup helpers

2. **FixtureBuilder**
    - Builder pattern implementation for test fixtures
    - Fluent API for configuring test fixtures
    - Methods for setting up repositories, users, permissions, and authentication

### Utility Classes

1. **AuthenticationUtils**
    - JWT token generation and validation
    - Predefined claims for different user roles
    - Test security parameters

2. **UserUtils**
    - Test user data generation

3. **QueryParamsUtils**
    - Test query parameter generation
    - Valid, invalid, and special case query parameters

## Using the Test Abstractions

### Writing a New Integration Test

```csharp
public sealed class MyControllerIntegrationTests : BaseIntegrationTest
{
    public MyControllerIntegrationTests(MockedRepositoryFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task MyEndpoint_WithValidInput_ReturnsExpectedResult()
    {
        // Setup test data
        List<User> users = UserUtils.GetValidUsers().ToList();
        
        // Setup fixture with admin user
        SetupAdminUser(users);

        // Execute request
        HttpResponseMessage response = await Client.GetAsync("/api/v1/my-endpoint");

        // Assert response
        await AssertResponseStatusCode(response, HttpStatusCode.OK);
        MyDto? result = await ReadResponseContent<MyDto>(response);
        
        // Assert result
        result.Should().NotBeNull();
        // Additional assertions...
    }
}
```

### Writing a New Authentication Test

```csharp
public sealed class MyAuthenticationTests : AuthenticationTestBase
{
    public MyAuthenticationTests(MockedRepositoryFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Authenticate_WithSpecialCase_ReturnsExpectedResult()
    {
        // Setup authentication
        SetupValidAuthentication("username", "password", new User { Id = 1 });

        // Execute request
        HttpResponseMessage response = await RequestToken("username", "password");

        // Assert response
        await AssertSuccessfulTokenResponse(response);
    }
}
```

## Benefits of This Structure

1. **Reduced Code Duplication**
    - Common setup code is centralized
    - HTTP request execution is standardized
    - Response assertions follow consistent patterns

2. **Improved Maintainability**
    - Changes to authentication logic only need to be made in one place
    - Test setup is more declarative and less imperative
    - Consistent patterns make tests easier to understand

3. **Easier Test Writing**
    - New tests can focus on specific test logic rather than boilerplate
    - Helper methods provide clear intent
    - Base classes handle common concerns 