#region

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

#endregion

namespace zora.API.Controllers;

[ApiController]
[Route("api/v1/authorisation")]
[Produces("application/json")]
[Consumes("application/json")]
[ProducesResponseType<int>(StatusCodes.Status200OK)]
public class AuthorisationController : ControllerBase
{
    private readonly IAuthorisationService _authorisationService;
    private readonly ILogger<AuthorisationController> _logger;

    public AuthorisationController(IAuthorisationService authorisationService, ILogger<AuthorisationController> logger)
    {
        this._authorisationService = authorisationService;
        this._logger = logger;
    }

    [Authorize]
    [HttpGet("is-authorised")]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    public async Task<IActionResult> isAuthorised() => null;
}
