#region

using Microsoft.AspNetCore.Mvc;
using zora.Infrastructure.DataSeed;

#endregion

namespace zora.API.Controllers;

[ApiController]
[Route("api/v1/seed")]
[Produces("application/json")]
[Consumes("application/json")]
[ProducesResponseType<int>(StatusCodes.Status200OK)]
public sealed class SeedingController : ControllerBase
{
    private readonly IDataSeeder _dataSeeder;
    private readonly ILogger<SeedingController> _logger;

    public SeedingController(IDataSeeder dataSeeder, ILogger<SeedingController> logger)
    {
        this._dataSeeder = dataSeeder;
        this._logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> Seed()
    {
        try
        {
            await this._dataSeeder.SeedAsync();
            return this.Ok(true);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error during seeding");
            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
