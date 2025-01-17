#region

using System.ComponentModel;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zora.Core.Domain;
using zora.Core.DTOs;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Requests.Interfaces;
using zora.Core.DTOs.Responses;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.API.Controllers;

/// <summary>
///     Controller for managing assets.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/assets")]
[Produces("application/json")]
[Consumes("application/json")]
[Description("Assets API")]
public sealed class AssetController : BaseCrudController<Asset, CreateAssetDto, UpdateAssetDto, AssetResponseDto,
    DynamicQueryAssetParamsDto>, IZoraService
{
    /// <summary>
    ///     Service for managing assets.
    /// </summary>
    private readonly IAssetService _assetService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AssetController" /> class.
    /// </summary>
    /// <param name="assetService">The service for managing assets.</param>
    /// <param name="queryService">The service for handling queries.</param>
    /// <param name="roleService">The service for managing roles.</param>
    /// <param name="logger">The logger.</param>
    public AssetController(
        IAssetService assetService,
        IQueryService queryService,
        IRoleService roleService,
        ILogger<AssetController> logger)
        : base(logger, roleService, queryService)
    {
        this._assetService = assetService;
    }

    /// <summary>
    ///     Gets all assets.
    /// </summary>
    /// <param name="queryParams">The query parameters.</param>
    /// <returns>A list of assets.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(AssetResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Tags("Assets")]
    [Description("Get all assets using query parameters")]
    public override async Task<ActionResult<AssetResponseDto>> Get([FromQuery] QueryParamsDto queryParams)
    {
        try
        {
            ActionResult authResult = this.HandleAdminAuthorization();
            if (authResult is UnauthorizedResult)
            {
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
    ///     Creates a new asset.
    /// </summary>
    /// <param name="createDto">The asset data to create.</param>
    /// <returns>The created asset.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Asset), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Tags("Assets")]
    [Description("Create a new asset")]
    [Authorize]
    public override async Task<ActionResult<Asset>> Create([FromBody] CreateAssetDto createDto)
    {
        try
        {
            Result<CreateAssetDto> assetValidationResult = this._assetService.ValidateDto(createDto);

            if (assetValidationResult.IsFailed)
            {
                this.Logger.LogError("Asset creation request is invalid: {Error}", assetValidationResult.Errors);
                return this.BadRequest(assetValidationResult.Errors);
            }

            Result<Asset> createdAssetResult = await this._assetService.CreateAsync(createDto);

            if (createdAssetResult.IsFailed)
            {
                this.Logger.LogError("Error creating asset: {Error}", createdAssetResult.Errors);
                return this.StatusCode(StatusCodes.Status500InternalServerError, createdAssetResult.Errors);
            }

            return this.CreatedAtAction(nameof(AssetController.Get), new { id = createdAssetResult.Value.Id },
                createdAssetResult.Value);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error creating asset");
            return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    ///     Updates an existing asset.
    /// </summary>
    /// <param name="id">The ID of the asset to update.</param>
    /// <param name="updateDto">The asset data to update.</param>
    /// <returns>The updated asset.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Asset), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Tags("Assets")]
    [Description("Update an existing asset")]
    public override async Task<ActionResult<Asset>> Update(long id, [FromBody] UpdateAssetDto updateDto)
    {
        try
        {
            ActionResult authResult = this.HandleAdminAuthorization();
            if (authResult is UnauthorizedResult)
            {
                return this.Unauthorized();
            }

            Result<UpdateAssetDto> dtoResult = this._assetService.ValidateDto(updateDto);

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

            Result<Asset> updatedAssetResult = await this._assetService.UpdateAsync(id, updateDto);

            if (updatedAssetResult.IsFailed)
            {
                if (updatedAssetResult.Errors.Any(error =>
                        error.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)))
                {
                    this.Logger.LogWarning("Asset not found: {Id}", id);
                    return this.NotFound($"Asset with ID {id} not found.");
                }

                this.Logger.LogError("Error updating asset: {Error}", updatedAssetResult.Errors);
                return this.StatusCode(StatusCodes.Status500InternalServerError, updatedAssetResult.Errors);
            }

            return this.Ok(updatedAssetResult.Value);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error updating asset with ID {Id}", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    ///     Deletes an existing asset.
    /// </summary>
    /// <param name="id">The ID of the asset to delete.</param>
    /// <returns>True if the asset was deleted successfully, otherwise false.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Tags("Assets")]
    [Description("Delete an existing asset")]
    public override async Task<ActionResult<bool>> Delete(long id)
    {
        try
        {
            ActionResult authResult = this.HandleAdminAuthorization();
            if (authResult is UnauthorizedResult)
            {
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
    [Description("Find assets based on specific criteria")]
    public override async Task<ActionResult<AssetResponseDto>> Find([FromQuery] QueryParamsDto findParams)
    {
        try
        {
            ActionResult authResult = this.HandleAdminAuthorization();
            if (authResult is UnauthorizedResult)
            {
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