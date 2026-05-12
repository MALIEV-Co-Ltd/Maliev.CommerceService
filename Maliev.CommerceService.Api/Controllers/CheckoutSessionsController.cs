using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.CommerceService.Api.Authorization;
using Maliev.CommerceService.Application.Dtos;
using Maliev.CommerceService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.CommerceService.Api.Controllers;

/// <summary>
/// Checkout session endpoints.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("commerce/v{version:apiVersion}/checkout-sessions")]
public sealed class CheckoutSessionsController(ICommerceService commerceService) : ControllerBase
{
    private readonly ICommerceService _commerceService = commerceService;

    /// <summary>
    /// Creates a checkout session.
    /// </summary>
    [HttpPost]
    [RequirePermission(CommercePermissions.CheckoutsCreate)]
    public async Task<ActionResult<CheckoutSessionResponse>> CreateCheckout([FromBody] CreateCheckoutSessionRequest request, CancellationToken cancellationToken)
    {
        var session = await _commerceService.CreateCheckoutSessionAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetCheckout), new { id = session.Id }, session);
    }

    /// <summary>
    /// Gets a checkout session.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(CommercePermissions.CheckoutsCreate)]
    public async Task<ActionResult<CheckoutSessionResponse>> GetCheckout(Guid id, CancellationToken cancellationToken)
    {
        var session = await _commerceService.GetCheckoutSessionAsync(id, cancellationToken);
        return session is null ? NotFound() : Ok(session);
    }

    /// <summary>
    /// Completes a checkout session and creates a shop order.
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [RequirePermission(CommercePermissions.OrdersCreate)]
    public async Task<ActionResult<StoreOrderResponse>> CompleteCheckout(Guid id, CancellationToken cancellationToken)
    {
        var order = await _commerceService.CompleteCheckoutAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }
}
