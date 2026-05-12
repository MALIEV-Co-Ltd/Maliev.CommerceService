using System.ComponentModel.DataAnnotations;

namespace Maliev.CommerceService.Domain.Entities;

/// <summary>
/// Checkout session created from a cart.
/// </summary>
public class CheckoutSession
{
    /// <summary>Gets or sets the checkout session identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the cart identifier.</summary>
    public Guid CartId { get; set; }

    /// <summary>Gets or sets the shared CustomerService customer identifier.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the checkout status.</summary>
    [Required]
    [MaxLength(40)]
    public string Status { get; set; } = CheckoutStatus.Open;

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

    /// <summary>Gets or sets the expiration timestamp.</summary>
    public DateTimeOffset ExpiresAtUtc { get; set; } = DateTimeOffset.UtcNow.AddHours(24);

    /// <summary>Gets or sets the cart.</summary>
    public Cart? Cart { get; set; }
}
