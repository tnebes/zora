#region

using System.ComponentModel;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zora.Core.Domain;
using zora.Core.DTOs.Assets;
using zora.Core.DTOs.Requests;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.API.Controllers;

/// <summary>
///     Controller for managing assets in the system.
///     Provides CRUD operations for assets with authorization checks.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/assets")]
[Produces("application/json")]
[Description("Assets API")]
public sealed class AssetController : BaseCrudController<Asset, CreateAssetDto, UpdateAssetDto, AssetDto, AssetResponseDto,
    DynamicQueryAssetParamsDto>, IZoraService
{
    /// <summary>
    ///     Service for managing assets.
    /// </summary>
    private readonly IAssetService _assetService;

    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AssetController" /> class.
    /// </summary>
    /// <param name="assetService">The service for managing assets.</param>
    /// <param name="queryService">The service for handling queries.</param>
    /// <param name="roleService">The service for managing roles.</param>
    /// <param name="jwtService">The service for managing JWT tokens.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public AssetController(
        IAssetService assetService,
        IQueryService queryService,
        IRoleService roleService,
        IJwtService jwtService,
        ILogger<AssetController> logger,
        IMapper mapper)
        : base(logger, roleService, queryService)
    {
        this._assetService = assetService;
        this._jwtService = jwtService;
        this._mapper = mapper;
    }

    /// <summary>
    ///     Retrieves a paginated list of assets with support for filtering, sorting, and searching.
    /// </summary>
    /// <param name="queryParams">Query parameters including page number, page size, and search term</param>
    /// <returns>Paginated list of assets wrapped in AssetResponseDto</returns>
    [HttpGet]
    [ProducesResponseType(typeof(AssetResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Tags("Assets")]
    [Description("Retrieves a paginated list of assets. Supports filtering, sorting, and searching.")]
    public override async Task<ActionResult<AssetResponseDto>> Get([FromQuery] QueryParamsDto queryParams)
    {
        try
        {
            ActionResult authResult = this.HandleAdminAuthorization();
            if (authResult is UnauthorizedResult)
            {
                this.LogUnauthorisedAccess(this.User);
                return this.Unauthorized();
            }

            this.NormalizeQueryParamsForAdmin(queryParams);

            Result<AssetResponseDto> assetResponseResult = await this._assetService.GetDtoAsync(queryParams);

            if (assetResponseResult.IsFailed)
            {
                this.Logger.LogError("Error getting assets: {Error}", assetResponseResult.Errors);
                return this.StatusCode(StatusCodes.Status500InternalServerError, assetResponseResult.Errors);
            }

            return this.Ok(assetResponseResult.Value);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error getting assets");
            return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    ///     Creates a new asset in the system.
    /// </summary>
    /// <param name="createDto">Data transfer object containing asset creation details</param>
    /// <returns>Created asset object</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Tags("Assets")]
    [Description("Creates a new asset.")]
    public override async Task<ActionResult<AssetDto>> Create([FromForm] CreateAssetDto createDto)
    {
        try
        {
            Result<CreateAssetDto> assetValidationResult = this._assetService.ValidateCreateAssetDto(createDto);

            if (assetValidationResult.IsFailed)
            {
                this.Logger.LogError("Asset creation request is invalid: {Error}", assetValidationResult.Errors);
                return this.BadRequest(assetValidationResult.Errors);
            }

            long userId = this._jwtService.GetCurrentUserId(this.User);
            Result<Asset> createdAssetResult = await this._assetService.CreateAsync(createDto, userId);

            if (createdAssetResult.IsFailed)
            {
                this.Logger.LogError("Error creating asset: {Error}", createdAssetResult.Errors);
                return this.StatusCode(StatusCodes.Status500InternalServerError, createdAssetResult.Errors);
            }

            AssetDto assetDto = this._mapper.Map<AssetDto>(createdAssetResult.Value);
            return this.CreatedAtAction(nameof(this.Get), new { id = createdAssetResult.Value.Id }, assetDto);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error creating asset");
            return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    ///     Updates an existing asset by ID.
    /// </summary>
    /// <param name="id">ID of the asset to update</param>
    /// <param name="updateDto">Data transfer object containing updated asset details</param>
    /// <returns>Updated asset object</returns>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Tags("Assets")]
    [Description("Updates an existing asset by ID.")]
    public override async Task<ActionResult<AssetDto>> Update(long id, [FromForm] UpdateAssetDto updateDto)
    {
        try
        {
            ActionResult authResult = this.HandleAdminAuthorization();
            if (authResult is UnauthorizedResult)
            {
                this.LogUnauthorisedAccess(this.User);
                return this.Unauthorized();
            }

            Result<UpdateAssetDto> dtoResult = this._assetService.ValidateUpdateAssetDto(updateDto);

            if (dtoResult.IsFailed)
            {
                this.Logger.LogError("Asset update request is invalid: {Error}", dtoResult.Errors);
                return this.BadRequest(dtoResult.Errors);
            }

            if (id <= 0L)
            {
                this.Logger.LogWarning("Invalid asset ID: {Id}", id);
                return this.BadRequest("Invalid asset ID.");
            }

            Result<Asset> existingAssetResult = await this._assetService.GetByIdAsync(id);

            if (existingAssetResult.IsFailed)
            {
                this.Logger.LogWarning("Asset not found with ID: {Id}", id);
                return this.NotFound($"Asset with ID {id} not found.");
            }

            long userId = this._jwtService.GetCurrentUserId(this.User);
            Result<Asset> updatedAssetResult = await this._assetService.UpdateAsync(id, updateDto, userId);

            if (updatedAssetResult.IsFailed)
            {
                this.Logger.LogError("Error updating asset: {Error}", updatedAssetResult.Errors);
                return this.StatusCode(StatusCodes.Status500InternalServerError, updatedAssetResult.Errors);
            }

            AssetDto assetDto = this._mapper.Map<AssetDto>(updatedAssetResult.Value);
            return this.Ok(assetDto);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error updating asset with ID {Id}", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    ///     Deletes an asset by ID.
    /// </summary>
    /// <param name="id">ID of the asset to delete</param>
    /// <returns>Boolean indicating success of the deletion operation</returns>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Tags("Assets")]
    [Description("Deletes an asset by ID.")]
    public override async Task<ActionResult<bool>> Delete(long id)
    {
        try
        {
            ActionResult authResult = this.HandleAdminAuthorization();
            if (authResult is UnauthorizedResult)
            {
                this.LogUnauthorisedAccess(this.HttpContext.User);
                return this.Unauthorized();
            }

            if (id <= 0)
            {
                this.Logger.LogWarning("Invalid asset ID: {Id}", id);
                return this.BadRequest("Invalid asset ID.");
            }

            bool isDeleted = await this._assetService.DeleteAsync(id);

            if (!isDeleted)
            {
                this.Logger.LogWarning("Asset not found or already deleted: {Id}", id);
                return this.NotFound($"Asset with ID {id} not found or already deleted.");
            }

            return this.Ok(true);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error deleting asset with ID {Id}", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    ///     Finds assets based on specific criteria.
    /// </summary>
    /// <param name="findParams">The find parameters.</param>
    /// <returns>A list of assets matching the criteria.</returns>
    [HttpGet("find")]
    [ProducesResponseType(typeof(AssetResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Tags("Assets")]
    [Description("Find assets that contain any of specific criteria")]
    public override async Task<ActionResult<AssetResponseDto>> Find([FromQuery] QueryParamsDto findParams)
    {
        try
        {
            ActionResult authResult = this.HandleAdminAuthorization();
            if (authResult is UnauthorizedResult)
            {
                this.LogUnauthorisedAccess(this.HttpContext.User);
                return this.Unauthorized();
            }

            if (findParams == null)
            {
                this.Logger.LogWarning("Find parameters are null.");
                return this.BadRequest("Find parameters cannot be null.");
            }

            Result<AssetResponseDto> findResult = await this._assetService.FindAsync(findParams);

            if (findResult.IsFailed)
            {
                this.Logger.LogError("Error finding assets: {Error}", findResult.Errors);
                return this.StatusCode(StatusCodes.Status500InternalServerError, findResult.Errors);
            }

            return this.Ok(findResult.Value);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error finding assets");
            return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    ///     Searches assets using dynamic query parameters.
    /// </summary>
    /// <param name="searchParams">The search parameters.</param>
    /// <returns>A list of assets matching the search criteria.</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(AssetResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Tags("Assets")]
    [Description("Search assets using dynamic query parameters")]
    public override async Task<ActionResult<AssetResponseDto>> Search(
        [FromQuery] DynamicQueryAssetParamsDto searchParams)
    {
        try
        {
            ActionResult authResult = this.HandleAdminAuthorization();
            if (authResult is UnauthorizedResult)
            {
                this.LogUnauthorisedAccess(this.HttpContext.User);
                return this.Unauthorized();
            }

            Result<AssetResponseDto> searchResult = await this._assetService.SearchAsync(searchParams);

            if (searchResult.IsFailed)
            {
                this.Logger.LogError("Error searching assets: {Error}", searchResult.Errors);
                return this.StatusCode(StatusCodes.Status500InternalServerError, searchResult.Errors);
            }

            return this.Ok(searchResult.Value);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error searching assets");
            return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
