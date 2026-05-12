namespace Maliev.CommerceService.Domain.Entities;

/// <summary>
/// Line item in a shop order.
/// </summary>
public class StoreOrderLine
{
    /// <summary>Gets or sets the line identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the order identifier.</summary>
    public Guid StoreOrderId { get; set; }

    /// <summary>Gets or sets the product variant identifier.</summary>
    public Guid ProductVariantId { get; set; }

    /// <summary>Gets or sets the SKU snapshot.</summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>Gets or sets the title snapshot.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the quantity.</summary>
    public int Quantity { get; set; }

    /// <summary>Gets or sets the unit price snapshot.</summary>
    public decimal UnitPriceAmount { get; set; }

    /// <summary>Gets or sets the currency snapshot.</summary>
    public string Currency { get; set; } = "THB";

    /// <summary>Gets or sets the store order.</summary>
    public StoreOrder? StoreOrder { get; set; }
}
