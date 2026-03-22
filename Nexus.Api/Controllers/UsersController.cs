using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Dto.Response;
using Nexus.Application.Dto.Users;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Api.Extensions;

namespace Nexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "users.view")]
    public async Task<ActionResult<Response<IReadOnlyList<UserDto>>>> GetAll(CancellationToken ct = default)
    {
        var result = await userService.GetAllAsync(ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "users.view")]
    public async Task<ActionResult<Response<UserDto>>> GetById(long id, CancellationToken ct = default)
    {
        var result = await userService.GetByIdAsync(id, ct);
        return result.ToActionResult();
    }

    [HttpGet("search")]
    [Authorize(Policy = "users.view")]
    public async Task<ActionResult<ResponsePagination<UserDto>>> Search([FromQuery] UserSearchRequest request,
        CancellationToken ct = default)
    {
        var result = await userService.SearchAsync(request, ct);
        return result.ToActionResult();
    }

    [HttpGet("username/{username}")]
    [Authorize(Policy = "users.view")]
    public async Task<ActionResult<Response<UserDto>>> GetByUsername(string username, CancellationToken ct = default)
    {
        var result = await userService.GetByUsernameAsync(username, ct);
        return result.ToActionResult();
    }

    [HttpGet("company/{companyId:long}")]
    [Authorize(Policy = "users.view")]
    public async Task<ActionResult<Response<IReadOnlyList<UserDto>>>> GetByCompany(long companyId,
        CancellationToken ct = default)
    {
        var result = await userService.GetByCompanyAsync(companyId, ct);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "users.manage")]
    public async Task<ActionResult<Response<UserDto>>> Create([FromBody] CreateUserDto dto,
        CancellationToken ct = default)
    {
        var result = await userService.CreateAsync(dto, ct);
        return result.ToCreatedAtActionResult(nameof(GetById), new { id = result.Data!.Id });
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "users.manage")]
    public async Task<ActionResult<Response<UserDto>>> Update(long id, [FromBody] UpdateUserDto dto,
        CancellationToken ct = default)
    {
        var result = await userService.UpdateAsync(id, dto, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "users.manage")]
    public async Task<ActionResult<Response<bool>>> Delete(long id, CancellationToken ct = default)
    {
        var result = await userService.DeleteAsync(id, ct);
        return result.ToNoContentResult();
    }

    [HttpPost("{id:long}/roles")]
    [Authorize(Policy = "users.manage")]
    public async Task<ActionResult<Response<UserDto>>> AssignRole(long id, [FromBody] AssignRoleDto dto,
        CancellationToken ct = default)
    {
        var result = await userService.AssignRoleAsync(id, dto, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:long}/roles/{roleId:long}")]
    [Authorize(Policy = "users.manage")]
    public async Task<ActionResult<Response<bool>>> RemoveRole(long id, long roleId, CancellationToken ct = default)
    {
        var result = await userService.RemoveRoleAsync(id, roleId, ct);
        return result.ToActionResult();
    }
}
