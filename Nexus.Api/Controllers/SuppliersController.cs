using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Dto.Suppliers;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Api.Extensions;

namespace Nexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController(ISupplierService supplierService, IClaimsExtractor claimsExtractor) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "suppliers.view")]
    public async Task<ActionResult<Response<IReadOnlyList<SupplierDto>>>> GetAll(CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await supplierService.GetAllAsync(companyId, ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "suppliers.view")]
    public async Task<ActionResult<Response<SupplierDto>>> GetById(long id, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await supplierService.GetByIdAsync(id, companyId, ct);
        return result.ToActionResult();
    }

    [HttpGet("search")]
    [Authorize(Policy = "suppliers.view")]
    public async Task<ActionResult<ResponsePagination<SupplierDto>>> Search([FromQuery] SupplierSearchRequest request,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var requestWithCompany = request with { CompanyId = companyId };
        var result = await supplierService.SearchAsync(requestWithCompany, ct);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "suppliers.manage")]
    public async Task<ActionResult<Response<SupplierDto>>> Create([FromBody] CreateSupplierDto dto,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await supplierService.CreateAsync(companyId, dto, ct);
        return result.ToCreatedAtActionResult(nameof(GetById), new { id = result.Data!.Id });
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "suppliers.manage")]
    public async Task<ActionResult<Response<SupplierDto>>> Update(long id,
        [FromBody] UpdateSupplierDto dto, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await supplierService.UpdateAsync(id, dto, companyId, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "suppliers.manage")]
    public async Task<ActionResult<Response<bool>>> Delete(long id, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await supplierService.DeleteAsync(id, companyId, ct);
        return result.ToNoContentResult();
    }
}
