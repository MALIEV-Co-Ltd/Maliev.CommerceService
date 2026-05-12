using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.CommerceService.Api.Authorization;
using Maliev.CommerceService.Application.Dtos;
using Maliev.CommerceService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.CommerceService.Api.Controllers;

/// <summary>
/// Product catalog endpoints.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("commerce/v{version:apiVersion}/products")]
public sealed class ProductsController(ICommerceService commerceService) : ControllerBase
{
    private readonly ICommerceService _commerceService = commerceService;

    /// <summary>
    /// Lists storefront products.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResponse<ProductSummaryResponse>>> ListProducts(
        [FromQuery] string? query = null,
        [FromQuery] string? collection = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        CancellationToken cancellationToken = default)
    {
        var result = await _commerceService.ListProductsAsync(query, collection, page, pageSize, includeDrafts: false, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lists products for employee catalog management.
    /// </summary>
    [HttpGet("manage")]
    [RequirePermission(CommercePermissions.ProductsRead)]
    public async Task<ActionResult<PagedResponse<ProductSummaryResponse>>> ListManagedProducts(
        [FromQuery] string? query = null,
        [FromQuery] string? collection = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        CancellationToken cancellationToken = default)
    {
        var result = await _commerceService.ListProductsAsync(query, collection, page, pageSize, includeDrafts: true, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a product by handle.
    /// </summary>
    [HttpGet("{handle}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductResponse>> GetProduct(string handle, CancellationToken cancellationToken)
    {
        var product = await _commerceService.GetProductAsync(handle, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    /// <summary>
    /// Gets a product by handle for employee catalog management.
    /// </summary>
    [HttpGet("manage/{handle}")]
    [RequirePermission(CommercePermissions.ProductsRead)]
    public async Task<ActionResult<ProductResponse>> GetManagedProduct(string handle, CancellationToken cancellationToken)
    {
        var product = await _commerceService.GetProductAsync(handle, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    /// <summary>
    /// Creates a product.
    /// </summary>
    [HttpPost]
    [RequirePermission(CommercePermissions.ProductsCreate)]
    public async Task<ActionResult<ProductResponse>> CreateProduct([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _commerceService.CreateProductAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetProduct), new { handle = product.Handle }, product);
    }

    /// <summary>
    /// Updates a product.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [RequirePermission(CommercePermissions.ProductsUpdate)]
    public async Task<ActionResult<ProductResponse>> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _commerceService.UpdateProductAsync(id, request, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    /// <summary>
    /// Updates a product publishing status.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [RequirePermission(CommercePermissions.ProductsUpdate)]
    public async Task<ActionResult<ProductResponse>> UpdateProductStatus(Guid id, [FromBody] UpdateProductStatusRequest request, CancellationToken cancellationToken)
    {
        var product = await _commerceService.UpdateProductStatusAsync(id, request, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    /// <summary>
    /// Archives a product.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(CommercePermissions.ProductsDelete)]
    public async Task<IActionResult> ArchiveProduct(Guid id, CancellationToken cancellationToken)
    {
        var archived = await _commerceService.ArchiveProductAsync(id, cancellationToken);
        return archived ? NoContent() : NotFound();
    }
}
