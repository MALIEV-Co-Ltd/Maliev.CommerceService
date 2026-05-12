using System.ComponentModel.DataAnnotations;

namespace Maliev.CommerceService.Domain.Entities;

/// <summary>
/// E-commerce shop order.
/// </summary>
public class StoreOrder
{
    /// <summary>Gets or sets the order identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the human-readable order number.</summary>
    [Required]
    [MaxLength(40)]
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the shared CustomerService customer identifier.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the checkout session identifier.</summary>
    public Guid CheckoutSessionId { get; set; }

    /// <summary>Gets or sets the order status.</summary>
    [Required]
    [MaxLength(40)]
    public string Status { get; set; } = StoreOrderStatus.PendingPayment;

    /// <summary>Gets or sets the total amount.</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Gets or sets the ISO currency code.</summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "THB";

    /// <summary>Gets or sets the shipping address snapshot.</summary>
    public string? ShippingAddressJson { get; set; }

    /// <summary>Gets or sets the billing address snapshot.</summary>
    public string? BillingAddressJson { get; set; }

    /// <summary>Gets or sets the created timestamp.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Gets or sets the updated timestamp.</summary>
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Gets the order lines.</summary>
    public List<StoreOrderLine> Lines { get; } = [];

    /// <summary>Gets or sets the checkout session.</summary>
    public CheckoutSession? CheckoutSession { get; set; }
}
