using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Dto.Access;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.UseCases;

namespace Nexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccessesController(IAccessService accessService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "accesses.view")]
    public async Task<ActionResult<Response<IReadOnlyList<AccessDto>>>> GetAll(CancellationToken ct = default)
    {
        var result = await accessService.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "accesses.view")]
    public async Task<ActionResult<Response<AccessDto>>> GetById(long id, CancellationToken ct = default)
    {
        var result = await accessService.GetByIdAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Policy = "accesses.manage")]
    public async Task<ActionResult<Response<AccessDto>>> Create([FromBody] CreateAccessDto dto,
        CancellationToken ct = default)
    {
        var result = await accessService.CreateAsync(dto, ct);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result)
            : Conflict(result);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "accesses.manage")]
    public async Task<ActionResult<Response<AccessDto>>> Update(long id, [FromBody] UpdateAccessDto dto,
        CancellationToken ct = default)
    {
        var result = await accessService.UpdateAsync(id, dto, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "accesses.manage")]
    public async Task<ActionResult<Response<bool>>> Delete(long id, CancellationToken ct = default)
    {
        var result = await accessService.DeleteAsync(id, ct);
        return result.Success ? NoContent() : NotFound(result);
    }
}
