using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Api.Extensions;

namespace Nexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KardexEntriesController(IKardexEntryService kardexService, IClaimsExtractor claimsExtractor)
    : ControllerBase
{
    [HttpGet("{id:long}")]
    [Authorize(Policy = "kardex.view")]
    public async Task<ActionResult<Response<KardexEntryDto>>> GetById(long id, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await kardexService.GetByIdAsync(id, companyId, ct);
        return result.ToActionResult();
    }

    [HttpGet("warehouse/{warehouseId:long}")]
    [Authorize(Policy = "kardex.view")]
    public async Task<ActionResult<Response<IReadOnlyList<KardexEntryDto>>>> GetByWarehouse(
        long warehouseId, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await kardexService.GetByWarehouseAsync(warehouseId, companyId, ct);
        return result.ToActionResult();
    }

    [HttpGet("search")]
    [Authorize(Policy = "kardex.view")]
    public async Task<ActionResult<ResponsePagination<KardexEntryDto>>> Search(
        [FromQuery] KardexEntrySearchRequest request, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await kardexService.SearchAsync(request, companyId, ct);
        return result.ToActionResult();
    }

    [HttpPost("reconcile")]
    [Authorize(Policy = "smartinventories.manage")]
    public async Task<ActionResult<Response<ReconciliationResultDto>>> Reconcile(
        [FromBody] ReconciliationRequestDto request, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await kardexService.ReconcileAsync(companyId, request.Correct, ct);
        return result.ToActionResult();
    }
}
