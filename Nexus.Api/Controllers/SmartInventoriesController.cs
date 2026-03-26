using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Api.Extensions;

namespace Nexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SmartInventoriesController(ISmartInventoryService smartInventoryService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "smartinventories.view")]
    public async Task<ActionResult<Response<IReadOnlyList<SmartInventoryDto>>>> GetAll([FromQuery] long companyId,
        CancellationToken ct = default)
    {
        var result = await smartInventoryService.GetAllAsync(companyId, ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "smartinventories.view")]
    public async Task<ActionResult<Response<SmartInventoryDto>>> GetById(long id, [FromQuery] long companyId,
        CancellationToken ct = default)
    {
        var result = await smartInventoryService.GetByIdAsync(id, companyId, ct);
        return result.ToActionResult();
    }

    [HttpGet("search")]
    [Authorize(Policy = "smartinventories.view")]
    public async Task<ActionResult<ResponsePagination<SmartInventoryDto>>> Search(
        [FromQuery] SmartInventorySearchRequest request,
        CancellationToken ct = default)
    {
        var result = await smartInventoryService.SearchAsync(request, ct);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "smartinventories.manage")]
    public async Task<ActionResult<Response<SmartInventoryDto>>> Create(
        [FromBody] CreateSmartInventoryDto dto, CancellationToken ct = default)
    {
        var result = await smartInventoryService.CreateAsync(dto, ct);
        return result.ToCreatedAtActionResult(nameof(GetById),
            new { id = result.Data!.Id, companyId = result.Data.CompanyId });
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "smartinventories.manage")]
    public async Task<ActionResult<Response<SmartInventoryDto>>> Update(long id,
        [FromBody] UpdateSmartInventoryDto dto, CancellationToken ct = default)
    {
        var result = await smartInventoryService.UpdateAsync(id, dto, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "smartinventories.manage")]
    public async Task<ActionResult<Response<bool>>> Delete(long id, [FromQuery] long companyId,
        CancellationToken ct = default)
    {
        var result = await smartInventoryService.DeleteAsync(id, companyId, ct);
        return result.ToNoContentResult();
    }
}
