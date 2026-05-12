using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.CommerceService.Api.Authorization;
using Maliev.CommerceService.Application.Dtos;
using Maliev.CommerceService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.CommerceService.Api.Controllers;

/// <summary>
/// Cart endpoints.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("commerce/v{version:apiVersion}/carts")]
public sealed class CartsController(ICommerceService commerceService) : ControllerBase
{
    private readonly ICommerceService _commerceService = commerceService;

    /// <summary>
    /// Creates a cart.
    /// </summary>
    [HttpPost]
    [RequirePermission(CommercePermissions.CartsCreate)]
    public async Task<ActionResult<CartResponse>> CreateCart([FromBody] CreateCartRequest request, CancellationToken cancellationToken)
    {
        var cart = await _commerceService.CreateCartAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetCart), new { id = cart.Id }, cart);
    }

    /// <summary>
    /// Gets a cart.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(CommercePermissions.CartsRead)]
    public async Task<ActionResult<CartResponse>> GetCart(Guid id, CancellationToken cancellationToken)
    {
        var cart = await _commerceService.GetCartAsync(id, cancellationToken);
        return cart is null ? NotFound() : Ok(cart);
    }

    /// <summary>
    /// Adds or updates a cart line.
    /// </summary>
    [HttpPost("{id:guid}/lines")]
    [RequirePermission(CommercePermissions.CartsUpdate)]
    public async Task<ActionResult<CartResponse>> UpsertLine(Guid id, [FromBody] UpsertCartLineRequest request, CancellationToken cancellationToken)
    {
        var cart = await _commerceService.UpsertCartLineAsync(id, request, cancellationToken);
        return cart is null ? NotFound() : Ok(cart);
    }

    /// <summary>
    /// Removes a cart line.
    /// </summary>
    [HttpDelete("{id:guid}/lines/{lineId:guid}")]
    [RequirePermission(CommercePermissions.CartsUpdate)]
    public async Task<ActionResult<CartResponse>> RemoveLine(Guid id, Guid lineId, CancellationToken cancellationToken)
    {
        var cart = await _commerceService.RemoveCartLineAsync(id, lineId, cancellationToken);
        return cart is null ? NotFound() : Ok(cart);
    }
}
