#region

using System.ComponentModel;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.API.Controllers;

[ApiController]
[Route("api/v1/roles")]
[Produces("application/json")]
[Consumes("application/json")]
[Description("Role API")]
public class RoleController : ControllerBase, IZoraService
{
    private readonly ILogger<RoleController> _logger;
    private readonly IMapper _mapper;
    private readonly IQueryService _queryService;
    private readonly IRoleService _roleService;

    public RoleController(
        IRoleService roleService,
        IQueryService queryService,
        IMapper mapper,
        ILogger<RoleController> logger)
    {
        this._roleService = roleService;
        this._queryService = queryService;
        this._mapper = mapper;
        this._logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(RoleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType<int>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<int>(StatusCodes.Status500InternalServerError)]
    [Tags("Roles")]
    [Description("Get all roles with pagination, searching and sorting support")]
    [Authorize]
    public async Task<IActionResult> GetRoles([FromQuery] QueryParamsDto queryParams)
    {
        try
        {
            if (!this._roleService.IsAdmin(this.HttpContext.User))
            {
                return this.Unauthorized();
            }

            this._queryService.NormaliseQueryParams(queryParams);

            RoleResponseDto roleResponse = await this._roleService.GetRolesDtoAsync(queryParams);
            return this.Ok(roleResponse);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting roles");
            return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
