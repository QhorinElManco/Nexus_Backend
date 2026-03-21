using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Interfaces.UseCases;

namespace Nexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InternalController(IDataSeedService dataSeedService) : ControllerBase
{
    [HttpPost("seed")]
    public async Task<IActionResult> Seed(CancellationToken ct = default)
    {
        try
        {
            await dataSeedService.SeedAsync(ct);
            return Ok(new { message = "Seed completed successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Seed failed", error = ex.Message });
        }
    }
}
