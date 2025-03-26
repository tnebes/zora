#region

using System.ComponentModel;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zora.Core.Domain;
using zora.Core.DTOs.Assets;
using zora.Core.DTOs.Permissions;
using zora.Core.DTOs.Requests;
using zora.Core.Enums;
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
public sealed class AssetController : BaseCrudController<Asset, CreateAssetDto, UpdateAssetDto, AssetDto,
    AssetResponseDto,
    DynamicQueryAssetParamsDto>, IZoraService
{
    private readonly IAssetPathService _assetPathService;

    /// <summary>
    ///     Service for managing assets.
    /// </summary>
    private readonly IAssetService _assetService;

    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;
    private readonly IPermissionService _permissionService;
    private readonly IUserRoleService _userRoleService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AssetController" /> class.
    /// </summary>
    /// <param name="assetService">The service for managing assets.</param>
    /// <param name="queryService">The service for handling queries.</param>
    /// <param name="roleService">The service for managing roles.</param>
    /// <param name="jwtService">The service for managing JWT tokens.</param>
    /// <param name="permissionService">The service for managing permissions.</param>
    /// <param name="userRoleService">The service for managing user roles.</param>
    /// <param name="assetPathService">The service for managing asset paths.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public AssetController(
        IAssetService assetService,
        IQueryService queryService,
        IRoleService roleService,
        IJwtService jwtService,
        IPermissionService permissionService,
        IUserRoleService userRoleService,
        IAssetPathService assetPathService,
        ILogger<AssetController> logger,
        IMapper mapper)
        : base(logger, roleService, queryService)
    {
        this._assetService = assetService;
        this._jwtService = jwtService;
        this._mapper = mapper;
        this._permissionService = permissionService;
        this._userRoleService = userRoleService;
        this._assetPathService = assetPathService;
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
            if (createDto.Asset == null || string.IsNullOrEmpty(createDto.Asset.FileName) ||
                !Path.HasExtension(createDto.Asset.FileName))
            {
                this.Logger.LogError("Asset file is missing or has no extension");
                return this.BadRequest("Asset file must be provided and must have a file extension.");
            }

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

            if (updateDto.File != null && (string.IsNullOrEmpty(updateDto.File.FileName) ||
                                           !Path.HasExtension(updateDto.File.FileName)))
            {
                this.Logger.LogError("Updated asset file has no extension");
                return this.BadRequest("Asset file must have a file extension.");
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

    [HttpGet("{id:long}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Tags("Assets")]
    [Description("Downloads an asset file if user has read permission on associated task")]
    public async Task<IActionResult> Download(long id)
    {
        try
        {
            Result<Asset> assetResult = await this._assetService.GetByIdAsync(id);

            if (assetResult.IsFailed)
            {
                this.Logger.LogWarning("Asset not found with ID: {Id}", id);
                return this.NotFound($"Asset with ID {id} not found.");
            }

            Asset asset = assetResult.Value;

            if (string.IsNullOrEmpty(asset.AssetPath) || !Path.HasExtension(asset.AssetPath))
            {
                this.Logger.LogError("Asset {Id} has invalid path or missing extension: {Path}", id, asset.AssetPath);
                return this.NotFound("Asset file is invalid or missing extension.");
            }

            long userId = this._jwtService.GetCurrentUserId(this.User);

            if (await this._userRoleService.IsAdminAsync(userId))
            {
                string fullPath = Path.Combine(this._assetPathService.GetAssetsBasePath(),
                    Path.GetFileName(asset.AssetPath));
                this.Logger.LogInformation(
                    "Admin user {UserId} attempting to download asset. Full path: {Path}, Base path: {BasePath}, Asset path: {AssetPath}",
                    userId, fullPath, this._assetPathService.GetAssetsBasePath(), asset.AssetPath);

                if (!System.IO.File.Exists(fullPath))
                {
                    this.Logger.LogError(
                        "Asset file not found at path: {Path}. Working directory: {WorkingDir}, Base directory: {BaseDir}",
                        fullPath,
                        Environment.CurrentDirectory,
                        AppDomain.CurrentDomain.BaseDirectory);
                    return this.NotFound("Asset file not found on server.");
                }

                string fileName = Path.GetFileName(asset.AssetPath);
                string contentType = this.GetContentType(fileName);

                return this.PhysicalFile(fullPath, contentType, fileName);
            }

            foreach (WorkItemAsset workItemAsset in asset.WorkItemAssets)
            {
                if (workItemAsset.WorkItem is ZoraTask task)
                {
                    PermissionRequestDto permissionRequest = new PermissionRequestDto
                    {
                        UserId = userId,
                        ResourceId = task.Id,
                        RequestedPermission = PermissionFlag.Read
                    };

                    bool hasPermission = await this._permissionService.HasDirectPermissionAsync(permissionRequest);

                    if (hasPermission)
                    {
                        string fullPath = Path.Combine(this._assetPathService.GetAssetsBasePath(),
                            Path.GetFileName(asset.AssetPath));

                        if (!System.IO.File.Exists(fullPath))
                        {
                            this.Logger.LogError("Asset file not found at path: {Path}", fullPath);
                            return this.NotFound("Asset file not found on server.");
                        }

                        string fileName = Path.GetFileName(asset.AssetPath);
                        string contentType = this.GetContentType(fileName);

                        return this.PhysicalFile(fullPath, contentType, fileName);
                    }
                }
            }

            this.Logger.LogWarning("User {UserId} does not have permission to download asset {AssetId}", userId, id);
            return this.Forbid();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error downloading asset with ID {Id}", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpGet("{id:long}/preview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Tags("Assets")]
    [Description("Gets a preview/thumbnail of an asset if user has read permission")]
    public async Task<IActionResult> GetPreview(long id)
    {
        try
        {
            Result<Asset> assetResult = await this._assetService.GetByIdAsync(id);

            if (assetResult.IsFailed)
            {
                this.Logger.LogWarning("Asset not found with ID: {Id}", id);
                return this.NotFound($"Asset with ID {id} not found.");
            }

            Asset asset = assetResult.Value;

            if (string.IsNullOrEmpty(asset.AssetPath) || !Path.HasExtension(asset.AssetPath))
            {
                this.Logger.LogError("Asset {Id} has invalid path or missing extension: {Path}", id, asset.AssetPath);
                return this.NotFound("Asset file is invalid or missing extension.");
            }

            long userId = this._jwtService.GetCurrentUserId(this.User);

            if (await this._userRoleService.IsAdminAsync(userId))
            {
                string fullPath = Path.Combine(this._assetPathService.GetAssetsBasePath(),
                    Path.GetFileName(asset.AssetPath));

                if (!System.IO.File.Exists(fullPath))
                {
                    this.Logger.LogError("Asset file not found at path: {Path}", fullPath);
                    return this.NotFound("Asset file not found on server.");
                }

                string fileName = Path.GetFileName(asset.AssetPath);
                string contentType = this.GetContentType(fileName);

                if (!contentType.StartsWith("image/"))
                {
                    return this.BadRequest("Preview is only available for image files");
                }

                return this.PhysicalFile(fullPath, contentType);
            }

            foreach (WorkItemAsset workItemAsset in asset.WorkItemAssets)
            {
                if (workItemAsset.WorkItem is ZoraTask task)
                {
                    PermissionRequestDto permissionRequest = new PermissionRequestDto
                    {
                        UserId = userId,
                        ResourceId = task.Id,
                        RequestedPermission = PermissionFlag.Read
                    };

                    bool hasPermission = await this._permissionService.HasDirectPermissionAsync(permissionRequest);

                    if (hasPermission)
                    {
                        string fullPath = Path.Combine(this._assetPathService.GetAssetsBasePath(),
                            Path.GetFileName(asset.AssetPath));

                        if (!System.IO.File.Exists(fullPath))
                        {
                            this.Logger.LogError("Asset file not found at path: {Path}", fullPath);
                            return this.NotFound("Asset file not found on server.");
                        }

                        string fileName = Path.GetFileName(asset.AssetPath);
                        string contentType = this.GetContentType(fileName);

                        if (!contentType.StartsWith("image/"))
                        {
                            return this.BadRequest("Preview is only available for image files");
                        }

                        return this.PhysicalFile(fullPath, contentType);
                    }
                }
            }

            this.Logger.LogWarning("User {UserId} does not have permission to preview asset {AssetId}", userId, id);
            return this.Forbid();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error getting preview for asset with ID {Id}", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    private string GetContentType(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".txt" => "text/plain",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }
}
