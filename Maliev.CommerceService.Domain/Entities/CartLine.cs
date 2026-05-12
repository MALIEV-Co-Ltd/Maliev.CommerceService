namespace Maliev.CommerceService.Domain.Entities;

/// <summary>
/// Item in a storefront cart.
/// </summary>
public class CartLine
{
    /// <summary>Gets or sets the cart line identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the cart identifier.</summary>
    public Guid CartId { get; set; }

    /// <summary>Gets or sets the product variant identifier.</summary>
    public Guid ProductVariantId { get; set; }

    /// <summary>Gets or sets the quantity.</summary>
    public int Quantity { get; set; }

    /// <summary>Gets or sets the unit price snapshot.</summary>
    public decimal UnitPriceAmount { get; set; }

    /// <summary>Gets or sets the currency snapshot.</summary>
    public string Currency { get; set; } = "THB";

    /// <summary>Gets or sets the SKU snapshot.</summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>Gets or sets the title snapshot.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the cart.</summary>
    public Cart? Cart { get; set; }

    /// <summary>Gets or sets the product variant.</summary>
    public ProductVariant? ProductVariant { get; set; }
}
