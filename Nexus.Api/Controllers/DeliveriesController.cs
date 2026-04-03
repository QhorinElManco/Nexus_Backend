using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Dto.Sales;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Api.Extensions;

namespace Nexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeliveriesController(IDeliveryService deliveryService, IClaimsExtractor claimsExtractor) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "deliveries.view")]
    public async Task<ActionResult<ResponsePagination<DeliveryDto>>> GetAll([FromQuery] DeliverySearchRequest request,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await deliveryService.SearchAsync(request, companyId, ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "deliveries.view")]
    public async Task<ActionResult<Response<DeliveryDto>>> GetById(long id, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await deliveryService.GetByIdAsync(id, companyId, ct);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "deliveries.manage")]
    public async Task<ActionResult<Response<DeliveryDto>>> Create([FromBody] CreateDeliveryDto dto,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var userId = claimsExtractor.GetCurrentUserId();
        var result = await deliveryService.CreateAsync(dto, companyId, userId, ct);
        return result.ToCreatedAtActionResult(nameof(GetById), new { id = result.Data!.Id });
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "deliveries.manage")]
    public async Task<ActionResult<Response<DeliveryDto>>> Update(long id, [FromBody] UpdateDeliveryDto dto,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await deliveryService.UpdateAsync(id, dto, companyId, ct);
        return result.ToActionResult();
    }
}
