using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Api.Extensions;

namespace Nexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "products.view")]
    public async Task<ActionResult<Response<IReadOnlyList<ProductDto>>>> GetAll(CancellationToken ct = default)
    {
        var result = await productService.GetAllAsync(ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = "products.view")]
    public async Task<ActionResult<Response<ProductDto>>> GetById(long id, CancellationToken ct = default)
    {
        var result = await productService.GetByIdAsync(id, ct);
        return result.ToActionResult();
    }

    [HttpGet("company/{companyId:long}")]
    [Authorize(Policy = "products.view")]
    public async Task<ActionResult<Response<IReadOnlyList<ProductDto>>>> GetByCompany(long companyId,
        CancellationToken ct = default)
    {
        var result = await productService.GetByCompanyAsync(companyId, ct);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Policy = "products.manage")]
    public async Task<ActionResult<Response<ProductDto>>> Create([FromBody] CreateProductDto dto,
        CancellationToken ct = default)
    {
        var result = await productService.CreateAsync(dto, ct);
        return result.ToCreatedAtActionResult(nameof(GetById), new { id = result.Data!.Id });
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = "products.manage")]
    public async Task<ActionResult<Response<ProductDto>>> Update(long id, [FromBody] UpdateProductDto dto,
        CancellationToken ct = default)
    {
        var result = await productService.UpdateAsync(id, dto, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "products.manage")]
    public async Task<ActionResult<Response<bool>>> Delete(long id, CancellationToken ct = default)
    {
        var result = await productService.DeleteAsync(id, ct);
        return result.ToNoContentResult();
    }
}