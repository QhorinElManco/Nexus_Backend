using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Dto.Response;
using Nexus.Application.Dto.Roles;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Api.Extensions;

namespace Nexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController(IRoleService roleService, IClaimsExtractor claimsExtractor) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "roles.view")]
    public async Task<ActionResult<Response<IReadOnlyList<RoleDto>>>> GetAll(CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await roleService.GetByCompanyAsync(companyId, ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "roles.view")]
    public async Task<ActionResult<Response<RoleDto>>> GetById(long id, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await roleService.GetByIdAsync(id, companyId, ct);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "roles.manage")]
    public async Task<ActionResult<Response<RoleDto>>> Create([FromBody] CreateRoleDto dto,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await roleService.CreateAsync(dto, companyId, ct);
        return result.ToCreatedAtActionResult(nameof(GetById), new { id = result.Data!.Id });
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "roles.manage")]
    public async Task<ActionResult<Response<RoleDto>>> Update(long id, [FromBody] UpdateRoleDto dto,
        CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await roleService.UpdateAsync(id, dto, companyId, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "roles.manage")]
    public async Task<ActionResult<Response<bool>>> Delete(long id, CancellationToken ct = default)
    {
        var companyId = claimsExtractor.GetCurrentCompanyId();
        var result = await roleService.DeleteAsync(id, companyId, ct);
        return result.ToNoContentResult();
    }
}
