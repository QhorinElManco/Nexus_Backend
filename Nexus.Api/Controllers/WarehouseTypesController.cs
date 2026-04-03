using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Api.Extensions;

namespace Nexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseTypesController(IWarehouseTypeService warehouseTypeService, IClaimsExtractor claimsExtractor)
    : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "warehousetypes.view")]
    public async Task<ActionResult<Response<IReadOnlyList<WarehouseTypeDto>>>> GetAll(CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await warehouseTypeService.GetByCompanyAsync(companyId, ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "warehousetypes.view")]
    public async Task<ActionResult<Response<WarehouseTypeDto>>> GetById(long id, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await warehouseTypeService.GetByIdAsync(id, companyId, ct);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "warehousetypes.manage")]
    public async Task<ActionResult<Response<WarehouseTypeDto>>> Create([FromBody] CreateWarehouseTypeDto dto,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await warehouseTypeService.CreateAsync(dto, companyId, ct);
        return result.ToCreatedAtActionResult(nameof(GetById), new { id = result.Data!.Id });
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "warehousetypes.manage")]
    public async Task<ActionResult<Response<WarehouseTypeDto>>> Update(long id, [FromBody] UpdateWarehouseTypeDto dto,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await warehouseTypeService.UpdateAsync(id, dto, companyId, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "warehousetypes.manage")]
    public async Task<ActionResult<Response<bool>>> Delete(long id, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await warehouseTypeService.DeleteAsync(id, companyId, ct);
        return result.ToNoContentResult();
    }
}
