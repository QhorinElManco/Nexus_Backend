using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Dto.Customers;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.UseCases;

namespace Nexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController(ICustomerService customerService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "customers.view")]
    public async Task<ActionResult<Response<IReadOnlyList<CustomerDto>>>> GetAll(CancellationToken ct = default)
    {
        var result = await customerService.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "customers.view")]
    public async Task<ActionResult<Response<CustomerDto>>> GetById(long id, CancellationToken ct = default)
    {
        var result = await customerService.GetByIdAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("search")]
    [Authorize(Policy = "customers.view")]
    public async Task<ActionResult<ResponsePagination<CustomerDto>>> Search([FromQuery] CustomerSearchRequest request,
        CancellationToken ct = default)
    {
        var result = await customerService.SearchAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("taxid/{taxId}")]
    [Authorize(Policy = "customers.view")]
    public async Task<ActionResult<Response<CustomerDto>>> GetByTaxId(string taxId, CancellationToken ct = default)
    {
        var result = await customerService.GetByTaxIdAsync(taxId, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("company/{companyId:long}")]
    [Authorize(Policy = "customers.view")]
    public async Task<ActionResult<Response<IReadOnlyList<CustomerDto>>>> GetByCompany(long companyId,
        CancellationToken ct = default)
    {
        var result = await customerService.GetByCompanyAsync(companyId, ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "customers.manage")]
    public async Task<ActionResult<Response<CustomerDto>>> Create([FromBody] CreateCustomerDto dto,
        CancellationToken ct = default)
    {
        var result = await customerService.CreateAsync(dto, ct);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result)
            : Conflict(result);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "customers.manage")]
    public async Task<ActionResult<Response<CustomerDto>>> Update(long id, [FromBody] UpdateCustomerDto dto,
        CancellationToken ct = default)
    {
        var result = await customerService.UpdateAsync(id, dto, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "customers.manage")]
    public async Task<ActionResult<Response<bool>>> Delete(long id, CancellationToken ct = default)
    {
        var result = await customerService.DeleteAsync(id, ct);
        return result.Success ? NoContent() : NotFound(result);
    }
}
