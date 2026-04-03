using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Dto.Customers;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Api.Extensions;

namespace Nexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController(ICustomerService customerService, IClaimsExtractor claimsExtractor) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "customers.view")]
    public async Task<ActionResult<Response<IReadOnlyList<CustomerDto>>>> GetAll(CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await customerService.GetByCompanyAsync(companyId, ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "customers.view")]
    public async Task<ActionResult<Response<CustomerDto>>> GetById(long id, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await customerService.GetByIdAsync(id, companyId, ct);
        return result.ToActionResult();
    }

    [HttpGet("search")]
    [Authorize(Policy = "customers.view")]
    public async Task<ActionResult<ResponsePagination<CustomerDto>>> Search([FromQuery] CustomerSearchRequest request,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var requestWithCompany = request with { CompanyId = companyId };
        var result = await customerService.SearchAsync(requestWithCompany, ct);
        return result.ToActionResult();
    }

    [HttpGet("taxid/{taxId}")]
    [Authorize(Policy = "customers.view")]
    public async Task<ActionResult<Response<CustomerDto>>> GetByTaxId(string taxId, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await customerService.GetByTaxIdAsync(taxId, companyId, ct);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "customers.manage")]
    public async Task<ActionResult<Response<CustomerDto>>> Create([FromBody] CreateCustomerDto dto,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var dtoWithCompany = dto with { CompanyId = companyId };
        var result = await customerService.CreateAsync(dtoWithCompany, ct);
        return result.ToCreatedAtActionResult(nameof(GetById), new { id = result.Data!.Id });
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "customers.manage")]
    public async Task<ActionResult<Response<CustomerDto>>> Update(long id, [FromBody] UpdateCustomerDto dto,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await customerService.UpdateAsync(id, dto, companyId, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "customers.manage")]
    public async Task<ActionResult<Response<bool>>> Delete(long id, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await customerService.DeleteAsync(id, companyId, ct);
        return result.ToNoContentResult();
    }
}
