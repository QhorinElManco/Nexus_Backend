using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Api.Extensions;

namespace Nexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehousesController(IWarehouseService warehouseService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "warehouses.view")]
    public async Task<ActionResult<Response<IReadOnlyList<WarehouseDto>>>> GetAll([FromQuery] long companyId,
        CancellationToken ct = default)
    {
        var result = await warehouseService.GetAllAsync(companyId, ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "warehouses.view")]
    public async Task<ActionResult<Response<WarehouseDto>>> GetById(long id, [FromQuery] long companyId,
        CancellationToken ct = default)
    {
        var result = await warehouseService.GetByIdAsync(id, companyId, ct);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "warehouses.manage")]
    public async Task<ActionResult<Response<WarehouseDto>>> Create([FromBody] CreateWarehouseDto dto, 
        CancellationToken ct = default)
    {
        var result = await warehouseService.CreateAsync(dto, ct);
        return result.ToCreatedAtActionResult(nameof(GetById), new { id = result.Data!.Id, companyId = dto.CompanyId });
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "warehouses.manage")]
    public async Task<ActionResult<Response<WarehouseDto>>> Update(long id,
        [FromBody] UpdateWarehouseDto dto, CancellationToken ct = default)
    {
        var result = await warehouseService.UpdateAsync(id, dto, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "warehouses.manage")]
    public async Task<ActionResult<Response<bool>>> Delete(long id, CancellationToken ct = default)
    {
        var result = await warehouseService.DeleteAsync(id, ct);
        return result.ToNoContentResult();
    }
}