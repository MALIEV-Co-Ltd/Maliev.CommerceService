namespace Maliev.CommerceService.Domain.Entities;

/// <summary>
/// Store order lifecycle status.
/// </summary>
public static class StoreOrderStatus
{
    /// <summary>Order was created and is waiting for payment confirmation.</summary>
    public const string PendingPayment = "PendingPayment";

    /// <summary>Order is paid and waiting for fulfillment.</summary>
    public const string Paid = "Paid";

    /// <summary>Order is being prepared.</summary>
    public const string Processing = "Processing";

    /// <summary>Order was shipped.</summary>
    public const string Shipped = "Shipped";

    /// <summary>Order is complete.</summary>
    public const string Completed = "Completed";

    /// <summary>Order was cancelled.</summary>
    public const string Cancelled = "Cancelled";
}
