using Microsoft.AspNetCore.Mvc;
using Nexos.Application.Dto.Companies;
using Nexos.Application.Interfaces.UseCases;
using Nexos.Transversal.Common.Response;

namespace Nexos.Services.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController(
    ICompanyService companyService) : ControllerBase
{
    [HttpGet("{id:long}")]
    public async Task<ActionResult<Response<CompanyDto>>> GetById(long id, CancellationToken ct = default)
    {
        var result = await companyService.GetByIdAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet]
    public async Task<ActionResult<Response<IReadOnlyList<CompanyDto>>>> GetAll(CancellationToken ct = default)
    {
        var result = await companyService.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<ResponsePagination<CompanyDto>>> Search([FromQuery] CompanySearchRequest request,
        CancellationToken ct = default)
    {
        var result = await companyService.SearchAsync(request, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Response<CompanyDto>>> Create([FromBody] CreateCompanyDto dto,
        CancellationToken ct = default)
    {
        var result = await companyService.CreateAsync(dto, ct);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result)
            : Conflict(result);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<Response<CompanyDto>>> Update(long id, [FromBody] UpdateCompanyDto dto,
        CancellationToken ct = default)
    {
        var result = await companyService.UpdateAsync(id, dto, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<Response<bool>>> Delete(long id, CancellationToken ct = default)
    {
        var result = await companyService.DeleteAsync(id, ct);
        return result.Success ? NoContent() : NotFound(result);
    }
}
