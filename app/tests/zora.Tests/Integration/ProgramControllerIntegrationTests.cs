#region

using zora.Tests.TestFixtures.v2;

#endregion

namespace zora.Tests.Integration;

[Collection("TestCollectionV2")]
public sealed class ProgramControllerIntegrationTests : BaseIntegrationTest
{
    [Fact(DisplayName =
        "GIVEN a simple get request from an authorised user with read permission WHEN Get() is invoked THEN the controller returns the task DTO")]
    public async Task Get_WithValidRequest_ReturnsTaskDto()
    {
    }


    // #region GET Tests
    //
    // [Fact(DisplayName =
    //     "GIVEN a valid QueryParamsDto and an admin user WHEN Get() is invoked THEN the controller returns an OK result with the expected paginated project list")]
    // public async Task Get_WithValidQueryParamsAndAdminUser_ReturnsOkWithPaginatedProjectList()
    // {
    //     // Arrange
    //
    //     // Act
    //
    //     // Assert
    // }
    //
    // [Fact(DisplayName =
    //     "GIVEN a valid QueryParamsDto and a non-admin user WHEN Get() is invoked THEN the controller returns an Unauthorized result")]
    // public async Task Get_WithNonAdminUser_ReturnsUnauthorized()
    // {
    //     // Arrange
    //
    //     // Act
    //
    //     // Assert
    // }
    //
    // [Fact(DisplayName =
    //     "GIVEN a valid QueryParamsDto with extra large page size and admin user WHEN Get() is invoked THEN the controller does not normalise the query parameters and returns filtered results")]
    // public async Task Get_WithQueryParameters_NormalizeQueryParametersAndReturnFilteredResults()
    // {
    //     // Arrange
    //
    //     // Act
    //
    //     // Assert
    // }
    //
    // [Fact(DisplayName =
    //     "GIVEN a QueryParamsDto that triggers an exception WHEN Get() is invoked THEN the controller returns a 500 Internal Server Error")]
    // public async Task Get_WithExceptionAtRepositoryLevel_Returns500InternalServerError()
    // {
    //     // Arrange
    //
    //     // Act
    //
    //     // Assert
    // }
    //
    // [Fact(DisplayName =
    //     "GIVEN a valid project ID and an admin user WHEN GetById() is invoked THEN the controller returns an OK result with the requested project")]
    // public async Task GetById_WithValidIdAndAdminUser_ReturnsOkWithProject()
    // {
    //     // Arrange
    //
    //     // Act
    //
    //     // Assert
    // }
    //
    // [Fact(DisplayName =
    //     "GIVEN an invalid project ID WHEN GetById() is invoked THEN the controller returns a 404 Not Found")]
    // public async Task GetById_WithInvalidId_ReturnsNotFound()
    // {
    //     // Arrange
    //
    //     // Act
    //
    //     // Assert
    // }
    //
    // [Fact(DisplayName =
    //     "GIVEN a valid program ID WHEN GetByProgram() is invoked THEN the controller returns an OK result with projects for that program")]
    // public async Task GetByProgram_WithValidProgramId_ReturnsOkWithProjects()
    // {
    //     // Arrange
    //
    //     // Act
    //
    //     // Assert
    // }
    //
    // #endregion
    //
    // #region POST Tests
    //
    // [Fact(DisplayName =
    //     "GIVEN a valid CreateProjectDto and an authenticated user WHEN Create() is invoked THEN the controller returns a 201 Created with the new project")]
    // public async Task Create_WithValidData_ReturnsCreatedWithNewProject()
    // {
    //     // Arrange
    //
    //     // Act
    //
    //     // Assert
    // }
    //
    // [Fact(DisplayName =
    //     "GIVEN an invalid CreateProjectDto and an authenticated user WHEN Create() is invoked THEN the controller returns a 400 Bad Request")]
    // public async Task Create_WithInvalidData_ReturnsBadRequest()
    // {
    //     // Arrange
    //
    //     // Act
    //
    //     // Assert
    // }
    //
    // [Fact(DisplayName =
    //     "GIVEN a valid CreateProjectDto that causes an exception WHEN Create() is invoked THEN the controller returns a 500 Internal Server Error")]
    // public async Task Create_WithRepositoryException_Returns500InternalServerError()
    // {
    //     // Arrange
    //
    //     // Act
    //
    //     // Assert
    // }
    //
    // #endregion
    //
    // #region PUT Tests
    //
    // [Fact(DisplayName =
    //     "GIVEN a valid UpdateProjectDto and an admin user WHEN Update() is invoked THEN the controller returns a 200 OK with the updated project")]
    // public async Task Update_WithAdminUserAndValidData_ReturnsOkWithUpdatedProject()
    // {
    //     // Arrange
    //
    //     // Act
    //
    //     // Assert
    // }
    //
    // [Fact(DisplayName =
    //     "GIVEN a valid UpdateProjectDto and a non-admin user WHEN Update() is invoked THEN the controller returns an Unauthorized result")]
    // public async Task Update_WithNonAdminUser_ReturnsUnauthorized()
    // {
    //     // Arrange
    //
    //     // Act
    //
    //     // Assert
    // }
    //
    // [Fact(DisplayName =
    //     "GIVEN an invalid project ID WHEN Update() is invoked THEN the controller returns a 400 Bad Request")]
    // public async Task Update_WithInvalidProjectId_ReturnsBadRequest()
    // {
    //     // Arrange
    //
    //     // Act
    //
    //     // Assert
    // }
    //
    // [Fact(DisplayName =
    //     "GIVEN a non-existent project ID WHEN Update() is invoked THEN the controller returns a 404 Not Found")]
    // public async Task Update_WithNonExistentProjectId_ReturnsNotFound()
    // {
    //     // Arrange
    //
    //     // Act
    //
    //     // Assert
    // }
    //
    // [Fact(DisplayName =
    //     "GIVEN a valid project ID that triggers an exception WHEN Update() is invoked THEN the controller returns a 500 Internal Server Error")]
    // public async Task Update_WithRepositoryException_Returns500InternalServerError()
    // {
    //     // Arrange
    //
    //     // Act
    //
    //     // Assert
    // }
    //
    // #endregion
    //
    // #region DELETE Tests
    //
    // [Fact(DisplayName =
    //     "GIVEN a valid project ID and an admin user WHEN Delete() is invoked THEN the controller returns a 200 OK with true")]
    // public async Task Delete_WithAdminUserAndValidProjectId_ReturnsOkWithTrue()
    // {
    //     // Arrange
    //
    //     // Act
    //
    //     // Assert
    // }
    //
    // [Fact(DisplayName =
    //     "GIVEN a valid project ID and a non-admin user WHEN Delete() is invoked THEN the controller returns an Unauthorized result")]
    // public async Task Delete_WithNonAdminUser_ReturnsUnauthorized()
    // {
    //     // Arrange
    //
    //     // Act
    //
    //     // Assert
    // }
    //
    // [Fact(DisplayName =
    //     "GIVEN an invalid project ID WHEN Delete() is invoked THEN the controller returns a 400 Bad Request")]
    // public async Task Delete_WithInvalidProjectId_ReturnsBadRequest()
    // {
    //     // Arrange
    //
    //     // Act
    //
    //     // Assert
    // }
    //
    // [Fact(DisplayName =
    //     "GIVEN a non-existent project ID WHEN Delete() is invoked THEN the controller returns a 404 Not Found")]
    // public async Task Delete_WithNonExistentProjectId_ReturnsNotFound()
    // {
    //     // Arrange
    //
    //     // Act
    //
    //     // Assert
    // }
    //
    // [Fact(DisplayName =
    //     "GIVEN a valid project ID that triggers an exception WHEN Delete() is invoked THEN the controller returns a 500 Internal Server Error")]
    // public async Task Delete_WithRepositoryException_Returns500InternalServerError()
    // {
    //     // Arrange
    //
    //     // Act
    //
    //     // Assert
    // }
    //
    // #endregion
    //
    // #region ProjectTaskRelationship Tests
    //
    // [Fact(DisplayName =
    //     "GIVEN a valid project ID WHEN GetTasks() is invoked THEN the controller returns an OK result with tasks for that project")]
    // public async Task GetTasks_WithValidProjectId_ReturnsOkWithTasks()
    // {
    //     // Arrange
    //
    //     // Act
    //
    //     // Assert
    // }
    //
    // [Fact(DisplayName =
    //     "GIVEN an invalid project ID WHEN GetTasks() is invoked THEN the controller returns a 404 Not Found")]
    // public async Task GetTasks_WithInvalidProjectId_ReturnsNotFound()
    // {
    //     // Arrange
    //
    //     // Act
    //
    //     // Assert
    // }
    //
    // [Fact(DisplayName =
    //     "GIVEN a project ID that triggers an exception WHEN GetTasks() is invoked THEN the controller returns a 500 Internal Server Error")]
    // public async Task GetTasks_WithRepositoryException_Returns500InternalServerError()
    // {
    //     // Arrange
    //
    //     // Act
    //
    //     // Assert
    // }
    //
    // #endregion
}
