namespace Maliev.CommerceService.Domain.Entities;

/// <summary>
/// Checkout session lifecycle status.
/// </summary>
public static class CheckoutStatus
{
    /// <summary>Checkout session is open.</summary>
    public const string Open = "Open";

    /// <summary>Checkout session has completed.</summary>
    public const string Completed = "Completed";

    /// <summary>Checkout session has expired.</summary>
    public const string Expired = "Expired";
}
