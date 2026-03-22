using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Dto.Companies;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Api.Extensions;

namespace Nexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController(ICompanyService companyService) : ControllerBase
{
    [HttpGet("{id:long}")]
    [Authorize(Policy = "companies.view")]
    public async Task<ActionResult<Response<CompanyDto>>> GetById(long id, CancellationToken ct = default)
    {
        var result = await companyService.GetByIdAsync(id, ct);
        return result.ToActionResult();
    }

    [HttpGet]
    [Authorize(Policy = "companies.view")]
    public async Task<ActionResult<Response<IReadOnlyList<CompanyDto>>>> GetAll(CancellationToken ct = default)
    {
        var result = await companyService.GetAllAsync(ct);
        return result.ToActionResult();
    }

    [HttpGet("search")]
    [Authorize(Policy = "companies.view")]
    public async Task<ActionResult<ResponsePagination<CompanyDto>>> Search([FromQuery] CompanySearchRequest request,
        CancellationToken ct = default)
    {
        var result = await companyService.SearchAsync(request, ct);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "companies.manage")]
    public async Task<ActionResult<Response<CompanyDto>>> Create([FromBody] CreateCompanyDto dto,
        CancellationToken ct = default)
    {
        var result = await companyService.CreateAsync(dto, ct);
        return result.ToCreatedAtActionResult(nameof(GetById), new { id = result.Data!.Id });
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "companies.manage")]
    public async Task<ActionResult<Response<CompanyDto>>> Update(long id, [FromBody] UpdateCompanyDto dto,
        CancellationToken ct = default)
    {
        var result = await companyService.UpdateAsync(id, dto, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "companies.manage")]
    public async Task<ActionResult<Response<bool>>> Delete(long id, CancellationToken ct = default)
    {
        var result = await companyService.DeleteAsync(id, ct);
        return result.ToNoContentResult();
    }
}
