using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Dto.Sales;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Api.Extensions;

namespace Nexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VisitsController(IVisitService visitService, IClaimsExtractor claimsExtractor) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "visits.view")]
    public async Task<ActionResult<ResponsePagination<VisitDto>>> GetAll([FromQuery] VisitSearchRequest request,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await visitService.SearchAsync(request, companyId, ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "visits.view")]
    public async Task<ActionResult<Response<VisitDto>>> GetById(long id, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await visitService.GetByIdAsync(id, companyId, ct);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "visits.manage")]
    public async Task<ActionResult<Response<VisitDto>>> Create([FromBody] CreateVisitDto dto,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var userId = claimsExtractor.GetCurrentUserId();
        var result = await visitService.CreateAsync(dto, companyId, userId, ct);
        return result.ToCreatedAtActionResult(nameof(GetById), new { id = result.Data!.Id });
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "visits.manage")]
    public async Task<ActionResult<Response<VisitDto>>> Update(long id, [FromBody] UpdateVisitDto dto,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await visitService.UpdateAsync(id, dto, companyId, ct);
        return result.ToActionResult();
    }

    [HttpPut("{id:long}/checkout")]
    [Authorize(Policy = "visits.manage")]
    public async Task<ActionResult<Response<VisitDto>>> Checkout(long id, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await visitService.CheckoutAsync(id, companyId, ct);
        return result.ToActionResult();
    }

    [HttpPut("{id:long}/cancel")]
    [Authorize(Policy = "visits.manage")]
    public async Task<ActionResult<Response<VisitDto>>> Cancel(long id, [FromBody] CancelVisitRequest request,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await visitService.CancelAsync(id, request.Reason, companyId, ct);
        return result.ToActionResult();
    }
}

public record CancelVisitRequest(string Reason);
