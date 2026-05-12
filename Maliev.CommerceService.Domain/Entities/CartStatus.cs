namespace Maliev.CommerceService.Domain.Entities;

/// <summary>
/// Shopping cart lifecycle status.
/// </summary>
public static class CartStatus
{
    /// <summary>Cart can still be edited.</summary>
    public const string Active = "Active";

    /// <summary>Cart has entered checkout.</summary>
    public const string Checkout = "Checkout";

    /// <summary>Cart has been converted to an order.</summary>
    public const string Ordered = "Ordered";

    /// <summary>Cart was abandoned or expired.</summary>
    public const string Abandoned = "Abandoned";
}
