using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Dto.Sales;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Api.Extensions;

namespace Nexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController(IPaymentService paymentService, IClaimsExtractor claimsExtractor) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "payments.view")]
    public async Task<ActionResult<ResponsePagination<PaymentDto>>> GetAll([FromQuery] PaymentSearchRequest request,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await paymentService.SearchAsync(request, companyId, ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "payments.view")]
    public async Task<ActionResult<Response<PaymentDto>>> GetById(long id, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await paymentService.GetByIdAsync(id, companyId, ct);
        return result.ToActionResult();
    }

    [HttpGet("order/{orderId:long}")]
    [Authorize(Policy = "payments.view")]
    public async Task<ActionResult<Response<IReadOnlyList<PaymentDto>>>> GetByOrderId(long orderId,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await paymentService.GetByOrderIdAsync(orderId, companyId, ct);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "payments.manage")]
    public async Task<ActionResult<Response<PaymentDto>>> Create([FromBody] CreatePaymentDto dto,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var userId = claimsExtractor.GetCurrentUserId();
        var result = await paymentService.CreateAsync(dto, companyId, userId, ct);
        return result.ToCreatedAtActionResult(nameof(GetById), new { id = result.Data!.Id });
    }
}
