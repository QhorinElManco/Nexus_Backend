using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Dto.Sales;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Api.Extensions;

namespace Nexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderService orderService, IClaimsExtractor claimsExtractor) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "orders.view")]
    public async Task<ActionResult<ResponsePagination<OrderDto>>> GetAll([FromQuery] OrderSearchRequest request,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await orderService.SearchAsync(request, companyId, ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "orders.view")]
    public async Task<ActionResult<Response<OrderDto>>> GetById(long id, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await orderService.GetByIdAsync(id, companyId, ct);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "orders.manage")]
    public async Task<ActionResult<Response<OrderDto>>> Create([FromBody] CreateOrderDto dto,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var userId = claimsExtractor.GetCurrentUserId();
        var result = await orderService.CreateAsync(dto, companyId, userId, ct);
        return result.ToCreatedAtActionResult(nameof(GetById), new { id = result.Data!.Id });
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "orders.manage")]
    public async Task<ActionResult<Response<OrderDto>>> Update(long id, [FromBody] UpdateOrderDto dto,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await orderService.UpdateAsync(id, dto, companyId, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "orders.manage")]
    public async Task<ActionResult<Response<bool>>> Delete(long id, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await orderService.DeleteAsync(id, companyId, ct);
        return result.ToNoContentResult();
    }

    [HttpPost("{id:long}/details")]
    [Authorize(Policy = "orders.manage")]
    public async Task<ActionResult<Response<OrderDto>>> AddDetail(long id, [FromBody] CreateOrderDetailDto dto,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await orderService.AddDetailAsync(id, dto, companyId, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{orderId:long}/details/{detailId:long}")]
    [Authorize(Policy = "orders.manage")]
    public async Task<ActionResult<Response<bool>>> RemoveDetail(long orderId, long detailId,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await orderService.RemoveDetailAsync(orderId, detailId, companyId, ct);
        return result.ToNoContentResult();
    }
}
