using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Dto.Response;
using Nexus.Application.Dto.Roles;
using Nexus.Application.Interfaces.UseCases;

namespace Nexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController(IRoleService roleService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Response<IReadOnlyList<RoleDto>>>> GetAll(CancellationToken ct = default)
    {
        var result = await roleService.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<Response<RoleDto>>> GetById(long id, CancellationToken ct = default)
    {
        var result = await roleService.GetByIdAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("company/{companyId:long}")]
    public async Task<ActionResult<Response<IReadOnlyList<RoleDto>>>> GetByCompany(long companyId,
        CancellationToken ct = default)
    {
        var result = await roleService.GetByCompanyAsync(companyId, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Response<RoleDto>>> Create([FromBody] CreateRoleDto dto,
        CancellationToken ct = default)
    {
        var result = await roleService.CreateAsync(dto, ct);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result)
            : Conflict(result);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<Response<RoleDto>>> Update(long id, [FromBody] UpdateRoleDto dto,
        CancellationToken ct = default)
    {
        var result = await roleService.UpdateAsync(id, dto, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<Response<bool>>> Delete(long id, CancellationToken ct = default)
    {
        var result = await roleService.DeleteAsync(id, ct);
        return result.Success ? NoContent() : NotFound(result);
    }
}
