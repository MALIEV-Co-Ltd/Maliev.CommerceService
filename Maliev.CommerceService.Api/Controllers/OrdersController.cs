using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.CommerceService.Api.Authorization;
using Maliev.CommerceService.Application.Dtos;
using Maliev.CommerceService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.CommerceService.Api.Controllers;

/// <summary>
/// Shop order endpoints.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("commerce/v{version:apiVersion}/orders")]
public sealed class OrdersController(ICommerceService commerceService) : ControllerBase
{
    private readonly ICommerceService _commerceService = commerceService;

    /// <summary>
    /// Lists shop orders.
    /// </summary>
    [HttpGet]
    [RequirePermission(CommercePermissions.OrdersRead)]
    public async Task<ActionResult<IReadOnlyList<StoreOrderResponse>>> ListOrders([FromQuery] Guid? customerId = null, CancellationToken cancellationToken = default)
    {
        var orders = await _commerceService.ListOrdersAsync(customerId, cancellationToken);
        return Ok(orders);
    }

    /// <summary>
    /// Gets a shop order.
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission(CommercePermissions.OrdersRead)]
    public async Task<ActionResult<StoreOrderResponse>> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var order = await _commerceService.GetOrderAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }
}
