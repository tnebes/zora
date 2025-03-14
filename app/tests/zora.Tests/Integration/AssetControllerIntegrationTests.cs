#region

using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using zora.Core.Domain;
using zora.Core.DTOs.Assets;
using zora.Core.DTOs.Requests;
using zora.Core.Interfaces.Services;
using zora.Tests.TestFixtures.v2;
using zora.Tests.Utils;
using BaseIntegrationTest = zora.Tests.TestFixtures.v2.BaseIntegrationTest;

#endregion

namespace zora.Tests.Integration;

public sealed class AssetExceptionThrowingWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            Mock<IAssetService> mockAssetService = new Mock<IAssetService>();

            mockAssetService
                .Setup(service => service.FindAsync(It.IsAny<QueryParamsDto>()))
                .ThrowsAsync(new Exception("Simulated exception for testing"));

            mockAssetService
                .Setup(service => service.SearchAsync(It.IsAny<DynamicQueryAssetParamsDto>()))
                .ThrowsAsync(new Exception("Simulated exception for search testing"));

            mockAssetService
                .Setup(service => service.CreateAsync(It.IsAny<CreateAssetDto>()))
                .ThrowsAsync(new Exception("Simulated exception for create testing"));

            mockAssetService
                .Setup(service => service.UpdateAsync(It.IsAny<long>(), It.IsAny<UpdateAssetDto>()))
                .ThrowsAsync(new Exception("Simulated exception for update testing"));

            mockAssetService
                .Setup(service => service.ValidateCreateAssetDto(It.IsAny<CreateAssetDto>()))
                .Returns<CreateAssetDto>(dto => Result.Ok(dto));

            mockAssetService
                .Setup(service => service.ValidateUpdateAssetDto(It.IsAny<UpdateAssetDto>()))
                .Returns<UpdateAssetDto>(dto => Result.Ok(dto));

            mockAssetService
                .Setup(service => service.DeleteAsync(It.IsAny<long>()))
                .ThrowsAsync(new Exception("Simulated exception for delete testing"));

            services.Replace(ServiceDescriptor.Scoped(_ => mockAssetService.Object));

            services.Configure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
                options.DefaultScheme = "TestScheme";
            });

            services.AddAuthentication("TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });
        });
    }
}

[Collection("TestCollectionV2")]
public sealed class AssetControllerIntegrationTests : BaseIntegrationTest
{
    [Fact(DisplayName =
        "GIVEN a valid QueryParamsDto and an admin user WHEN Get() is invoked THEN the controller returns an OK result with the expected paginated asset list")]
    public async Task Get_WithValidQueryParamsAndAdminUser_ReturnsOkWithPaginatedAssetList()
    {
        await this.ClearDatabase();
        this.SetupAdminAuthentication();

        List<Asset> assets = AssetUtils.GetValidAssets().ToList();
        await this.SeedAssets(assets);

        QueryParamsDto queryParams = QueryUtils.QueryParamUtils.GetValidQueryParams();
        HttpResponseMessage response = await this.GetAssets(queryParams);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AssetResponseDto? responseAssets = await this.ReadResponseContent<AssetResponseDto>(response);

        responseAssets.Should().NotBeNull();
        responseAssets.Items.Should().NotBeNull();
        responseAssets.Items.Should().HaveCountGreaterThanOrEqualTo(assets.Count);
        responseAssets.Total.Should().BeGreaterThanOrEqualTo(assets.Count);
    }

    [Fact(DisplayName =
        "GIVEN a valid QueryParamsDto and a non-admin user WHEN Get() is invoked THEN the controller returns an Unauthorized result")]
    public async Task Get_WithNonAdminUser_ReturnsUnauthorized()
    {
        await this.ClearDatabase();
        this.SetupRegularUserAuthentication();

        QueryParamsDto queryParams = QueryUtils.QueryParamUtils.GetValidQueryParams();
        HttpResponseMessage response = await this.GetAssets(queryParams);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName =
        "GIVEN a valid QueryParamsDto with extra large page size and admin user WHEN Get() is invoked THEN the controller does not normalise the query parameters and returns filtered results")]
    public async Task Get_WithQueryParameters_NormalizeQueryParametersAndReturnFilteredResults()
    {
        await this.ClearDatabase();
        this.SetupAdminAuthentication();

        List<Asset> assets = AssetUtils.GetValidAssets().ToList();
        await this.SeedAssets(assets);

        QueryParamsDto queryParams = QueryUtils.QueryParamUtils.GetLargePageSizeQueryParams();

        HttpResponseMessage response = await this.GetAssets(queryParams);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AssetResponseDto? responseAssets = await this.ReadResponseContent<AssetResponseDto>(response);

        responseAssets.Should().NotBeNull();
        responseAssets.PageSize.Should().Be(10000);
    }

    [Fact(DisplayName =
        "GIVEN a QueryParamsDto that triggers an exception WHEN Get() is invoked THEN the controller returns a 500 Internal Server Error")]
    public async Task Get_WithExceptionAtRepositoryLevel_Returns500InternalServerError()
    {
        await using AssetExceptionThrowingWebApplicationFactory exceptionFactory = new();
        HttpClient exceptionClient = exceptionFactory.CreateClient();

        exceptionClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "Admin");

        QueryParamsDto queryParams = QueryUtils.QueryParamUtils.GetValidQueryParams();
        HttpResponseMessage response = await exceptionClient.GetAsync($"/api/v1/assets{queryParams.ToQueryString()}");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName =
        "GIVEN a valid CreateAssetDto and an authenticated user WHEN Create() is invoked THEN the controller returns a 201 Created with the new asset")]
    public async Task Create_WithValidData_ReturnsCreatedWithNewAsset()
    {
        await this.ClearDatabase();
        this.SetupAdminAuthentication();

        CreateAssetDto createAssetDto = AssetUtils.GetValidCreateAssetWithDataDto();
        HttpResponseMessage response = await this.CreateAsset(createAssetDto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        AssetDto? createdAsset = await this.ReadResponseContent<AssetDto>(response);

        createdAsset.Should().NotBeNull();
        createdAsset.Name.Should().Be(createAssetDto.Name);
        createdAsset.Description.Should().Be(createAssetDto.Description);
    }

    [Fact(DisplayName =
        "GIVEN an invalid CreateAssetDto and an authenticated user WHEN Create() is invoked THEN the controller returns a 400 Bad Request")]
    public async Task Create_WithInvalidData_ReturnsBadRequest()
    {
        await this.ClearDatabase();
        this.SetupAdminAuthentication();

        CreateAssetDto invalidAssetDto = AssetUtils.GetInvalidCreateAssetDto();

        HttpResponseMessage response = await this.CreateAsset(invalidAssetDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName =
        "GIVEN a valid CreateAssetDto that causes an exception WHEN Create() is invoked THEN the controller returns a 500 Internal Server Error")]
    public async Task Create_WithRepositoryException_Returns500InternalServerError()
    {
        using AssetExceptionThrowingWebApplicationFactory exceptionFactory = new();
        HttpClient exceptionClient = exceptionFactory.CreateClient();

        CreateAssetDto validAssetDto = AssetUtils.GetValidCreateAssetDto();

        MultipartFormDataContent formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent(validAssetDto.Name), "Name");
        if (validAssetDto.Description != null)
        {
            formContent.Add(new StringContent(validAssetDto.Description), "Description");
        }

        StreamContent fileContent = new StreamContent(validAssetDto.Asset.OpenReadStream());
        formContent.Add(fileContent, "Asset", validAssetDto.Asset.FileName);

        HttpResponseMessage response = await exceptionClient.PostAsync("/api/v1/assets", formContent);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName =
        "GIVEN a valid UpdateAssetDto and an admin user WHEN Update() is invoked THEN the controller returns a 200 OK with the updated asset")]
    public async Task Update_WithAdminUserAndValidData_ReturnsOkWithUpdatedAsset()
    {
        await this.ClearDatabase();
        this.SetupAdminAuthentication();

        List<Asset> assets = AssetUtils.GetValidAssets().ToList();
        await this.SeedAssets(assets);
        Asset existingAsset = assets.First();

        UpdateAssetDto updateAssetDto = AssetUtils.GetValidUpdateAssetDto();
        HttpResponseMessage response = await this.UpdateAsset(existingAsset.Id, updateAssetDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AssetDto? updatedAsset = await this.ReadResponseContent<AssetDto>(response);

        updatedAsset.Should().NotBeNull();
        updatedAsset.Id.Should().Be(existingAsset.Id);
        updatedAsset.Name.Should().Be(updateAssetDto.Name);
        updatedAsset.Description.Should().Be(updateAssetDto.Description);
    }

    [Fact(DisplayName =
        "GIVEN a valid UpdateAssetDto and a non-admin user WHEN Update() is invoked THEN the controller returns an Unauthorized result")]
    public async Task Update_WithNonAdminUser_ReturnsUnauthorized()
    {
        await this.ClearDatabase();
        this.SetupRegularUserAuthentication();

        List<Asset> assets = AssetUtils.GetValidAssets().ToList();
        await this.SeedAssets(assets);
        Asset existingAsset = assets.First();

        UpdateAssetDto updateAssetDto = AssetUtils.GetValidUpdateAssetDto();
        HttpResponseMessage response = await this.UpdateAsset(existingAsset.Id, updateAssetDto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName =
        "GIVEN an invalid asset ID WHEN Update() is invoked THEN the controller returns a 400 Bad Request")]
    public async Task Update_WithInvalidAssetId_ReturnsBadRequest()
    {
        await this.ClearDatabase();
        this.SetupAdminAuthentication();

        long invalidAssetId = -1;
        UpdateAssetDto updateAssetDto = AssetUtils.GetValidUpdateAssetDto();
        HttpResponseMessage response = await this.UpdateAsset(invalidAssetId, updateAssetDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName =
        "GIVEN a non-existent asset ID WHEN Update() is invoked THEN the controller returns a 404 Not Found")]
    public async Task Update_WithNonExistentAssetId_ReturnsNotFound()
    {
        await this.ClearDatabase();
        this.SetupAdminAuthentication();

        long nonExistentAssetId = 9999;
        UpdateAssetDto updateAssetDto = AssetUtils.GetValidUpdateAssetDto();
        HttpResponseMessage response = await this.UpdateAsset(nonExistentAssetId, updateAssetDto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName =
        "GIVEN a valid asset ID that triggers an exception WHEN Update() is invoked THEN the controller returns a 500 Internal Server Error")]
    public async Task Update_WithRepositoryException_Returns500InternalServerError()
    {
        using AssetExceptionThrowingWebApplicationFactory exceptionFactory = new();
        HttpClient exceptionClient = exceptionFactory.CreateClient();
        exceptionClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "Admin");

        long validAssetId = 1;
        UpdateAssetDto updateAssetDto = AssetUtils.GetValidUpdateAssetDto();

        MultipartFormDataContent formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent(updateAssetDto.Name), "Name");

        if (updateAssetDto.Description != null)
        {
            formContent.Add(new StringContent(updateAssetDto.Description), "Description");
        }

        formContent.Add(new StringContent(updateAssetDto.WorkItemId.ToString()), "WorkItemId");

        if (updateAssetDto.File != null)
        {
            StreamContent fileContent = new StreamContent(updateAssetDto.File.OpenReadStream());
            formContent.Add(fileContent, "File", updateAssetDto.File.FileName);
        }

        HttpResponseMessage response = await exceptionClient.PutAsync($"/api/v1/assets/{validAssetId}", formContent);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName =
        "GIVEN a valid asset ID and an admin user WHEN Delete() is invoked THEN the controller returns a 200 OK with true")]
    public async Task Delete_WithAdminUserAndValidAssetId_ReturnsOkWithTrue()
    {
        await this.ClearDatabase();
        this.SetupAdminAuthentication();

        List<Asset> assets = AssetUtils.GetValidAssets().ToList();
        await this.SeedAssets(assets);
        Asset existingAsset = assets.First();

        HttpResponseMessage response = await this.DeleteAsset(existingAsset.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        bool? result = await this.ReadResponseContent<bool>(response);

        result.Should().NotBeNull();
        result.Should().BeTrue();
    }

    [Fact(DisplayName =
        "GIVEN a valid asset ID and a non-admin user WHEN Delete() is invoked THEN the controller returns an Unauthorized result")]
    public async Task Delete_WithNonAdminUser_ReturnsUnauthorized()
    {
        await this.ClearDatabase();
        this.SetupRegularUserAuthentication();

        List<Asset> assets = AssetUtils.GetValidAssets().ToList();
        await this.SeedAssets(assets);
        Asset existingAsset = assets.First();

        HttpResponseMessage response = await this.DeleteAsset(existingAsset.Id);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName =
        "GIVEN an invalid asset ID WHEN Delete() is invoked THEN the controller returns a 400 Bad Request")]
    public async Task Delete_WithInvalidAssetId_ReturnsBadRequest()
    {
        await this.ClearDatabase();
        this.SetupAdminAuthentication();

        long invalidAssetId = -1;
        HttpResponseMessage response = await this.DeleteAsset(invalidAssetId);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName =
        "GIVEN a non-existent asset ID WHEN Delete() is invoked THEN the controller returns a 404 Not Found")]
    public async Task Delete_WithNonExistentAssetId_ReturnsNotFound()
    {
        await this.ClearDatabase();
        this.SetupAdminAuthentication();

        long nonExistentAssetId = 9999;
        HttpResponseMessage response = await this.DeleteAsset(nonExistentAssetId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName =
        "GIVEN a valid asset ID that triggers an exception WHEN Delete() is invoked THEN the controller returns a 500 Internal Server Error")]
    public async Task Delete_WithRepositoryException_Returns500InternalServerError()
    {
        await using AssetExceptionThrowingWebApplicationFactory exceptionFactory = new();
        HttpClient exceptionClient = exceptionFactory.CreateClient();
        exceptionClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "Admin");

        long validAssetId = 1;
        HttpResponseMessage response = await exceptionClient.DeleteAsync($"/api/v1/assets/{validAssetId}");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName =
        "GIVEN a valid QueryParamsDto and an admin user WHEN Find() is invoked THEN the controller returns an OK result with matching assets")]
    public async Task Find_WithAdminUserAndValidParams_ReturnsOkWithMatchingAssets()
    {
        await this.ClearDatabase();
        this.SetupAdminAuthentication();

        List<Asset> assets = AssetUtils.GetValidAssets().ToList();
        await this.SeedAssets(assets);

        QueryParamsDto queryParams = QueryUtils.QueryParamUtils.GetValidQueryParams();
        HttpResponseMessage response = await this.FindAssets(queryParams);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AssetResponseDto? responseAssets = await this.ReadResponseContent<AssetResponseDto>(response);

        responseAssets.Should().NotBeNull();
        responseAssets.Items.Should().NotBeNull();
        responseAssets.Items.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact(DisplayName =
        "GIVEN a valid DynamicQueryAssetParamsDto and an admin user WHEN Search() is invoked THEN the controller returns an OK result with matching assets")]
    public async Task Search_WithAdminUserAndValidParams_ReturnsOkWithMatchingAssets()
    {
        await this.ClearDatabase();
        this.SetupAdminAuthentication();

        List<Asset> assets = AssetUtils.GetValidAssets().ToList();
        await this.SeedAssets(assets);

        DynamicQueryAssetParamsDto searchParams = AssetUtils.GetValidDynamicQueryAssetParamsDto();
        HttpResponseMessage response = await this.SearchAssets(searchParams);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AssetResponseDto? responseAssets = await this.ReadResponseContent<AssetResponseDto>(response);

        responseAssets.Should().NotBeNull();
        responseAssets.Items.Should().NotBeNull();
    }

    [Fact(DisplayName =
        "GIVEN a DynamicQueryAssetParamsDto that triggers an exception WHEN Search() is invoked THEN the controller returns a 500 Internal Server Error")]
    public async Task Search_WithServiceException_Returns500InternalServerError()
    {
        await using AssetExceptionThrowingWebApplicationFactory exceptionFactory = new();
        HttpClient exceptionClient = exceptionFactory.CreateClient();
        exceptionClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "Admin");

        DynamicQueryAssetParamsDto searchParams = AssetUtils.GetValidDynamicQueryAssetParamsDto();
        HttpResponseMessage response =
            await exceptionClient.GetAsync($"/api/v1/assets/search{searchParams.ToQueryString()}");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
