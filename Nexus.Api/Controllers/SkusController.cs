using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Api.Extensions;

namespace Nexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkusController(ISkuService skuService, IClaimsExtractor claimsExtractor) : ControllerBase
{
    [HttpGet("{id:long}")]
    [Authorize(Policy = "skus.view")]
    public async Task<ActionResult<Response<SkuDto>>> GetById(long id, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await skuService.GetByIdAsync(id, companyId, ct);
        return result.ToActionResult();
    }

    [HttpGet("product/{productId:long}")]
    [Authorize(Policy = "skus.view")]
    public async Task<ActionResult<Response<IReadOnlyList<SkuDto>>>> GetByProduct(long productId,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await skuService.GetByProductAsync(productId, companyId, ct);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "skus.manage")]
    public async Task<ActionResult<Response<SkuDto>>> Create([FromBody] CreateSkuDto dto,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await skuService.CreateAsync(dto, companyId, ct);
        return result.ToCreatedAtActionResult(nameof(GetById), new { id = result.Data!.Id });
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "skus.manage")]
    public async Task<ActionResult<Response<SkuDto>>> Update(long id, [FromBody] UpdateSkuDto dto,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await skuService.UpdateAsync(id, dto, companyId, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "skus.manage")]
    public async Task<ActionResult<Response<bool>>> Delete(long id, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await skuService.DeleteAsync(id, companyId, ct);
        return result.ToNoContentResult();
    }
}
