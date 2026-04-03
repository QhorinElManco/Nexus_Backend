using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Api.Extensions;

namespace Nexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehousesController(IWarehouseService warehouseService, IClaimsExtractor claimsExtractor) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "warehouses.view")]
    public async Task<ActionResult<Response<IReadOnlyList<WarehouseDto>>>> GetAll(CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await warehouseService.GetAllAsync(companyId, ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "warehouses.view")]
    public async Task<ActionResult<Response<WarehouseDto>>> GetById(long id, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await warehouseService.GetByIdAsync(id, companyId, ct);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "warehouses.manage")]
    public async Task<ActionResult<Response<WarehouseDto>>> Create([FromBody] CreateWarehouseDto dto,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await warehouseService.CreateAsync(dto, companyId, ct);
        return result.ToCreatedAtActionResult(nameof(GetById), new { id = result.Data!.Id });
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "warehouses.manage")]
    public async Task<ActionResult<Response<WarehouseDto>>> Update(long id,
        [FromBody] UpdateWarehouseDto dto, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await warehouseService.UpdateAsync(id, dto, companyId, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "warehouses.manage")]
    public async Task<ActionResult<Response<bool>>> Delete(long id, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await warehouseService.DeleteAsync(id, companyId, ct);
        return result.ToNoContentResult();
    }
}
