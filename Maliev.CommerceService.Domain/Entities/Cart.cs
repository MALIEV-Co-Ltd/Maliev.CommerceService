using System.ComponentModel.DataAnnotations;

namespace Maliev.CommerceService.Domain.Entities;

/// <summary>
/// Customer or anonymous storefront cart.
/// </summary>
public class Cart
{
    /// <summary>Gets or sets the cart identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the shared CustomerService customer identifier.</summary>
    public Guid? CustomerId { get; set; }

    /// <summary>Gets or sets the anonymous browser cart key.</summary>
    [MaxLength(160)]
    public string? AnonymousKey { get; set; }

    /// <summary>Gets or sets the cart status.</summary>
    [Required]
    [MaxLength(40)]
    public string Status { get; set; } = CartStatus.Active;

    /// <summary>Gets or sets the ISO currency code.</summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "THB";

    /// <summary>Gets or sets the created timestamp.</summary>
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Gets or sets the updated timestamp.</summary>
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Gets the cart lines.</summary>
    public List<CartLine> Lines { get; } = [];
}
